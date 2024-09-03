using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using NextApi.Common;
using NextApi.Common.Abstractions.Security;
using NextApi.Common.Serialization;
using NextApi.Server.Request;
using NextApi.Server.Service;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Request processor from HTTP (in case we don't want to use SignalR as transport)
    /// </summary>
    public class NextApiHttp
    {
        private readonly INextApiUserAccessor _userAccessor;
        private readonly INextApiRequest _request;
        private readonly NextApiHandler _handler;

        /// <inheritdoc />
        public NextApiHttp(INextApiUserAccessor userAccessor, INextApiRequest request, NextApiHandler handler)
        {
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _handler = handler;
        }

        /// <summary>
        /// Entry point for NextApi HTTP requests
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessNextApiHttpRequestAsync(HttpContext context)
        {
            var form = context.Request.Form;

            _userAccessor.User = context.User;
            _request.FilesFromClient = form.Files;

            var command = new NextApiCommand
            {
                Service = form["Service"].FirstOrDefault(), Method = form["Method"].FirstOrDefault()
            };

            var serialization = form["Serialization"].FirstOrDefault();
            var useMessagePack = serialization == SerializationType.MessagePack.ToString();
            if (useMessagePack)
            {
                var argsFile = form.Files["Args"];
                using var memoryStream = new MemoryStream();
                await argsFile.CopyToAsync(memoryStream);
                var byteArray = memoryStream.ToArray();
                var args = MessagePackSerializer.Typeless.Deserialize(byteArray);
                command.Args = (INextApiArgument[]) args;
            }
            else
            {
                var argsString = form["Args"].FirstOrDefault();
                command.Args = string.IsNullOrEmpty(argsString)
                    ? null
                    : JsonConvert.DeserializeObject<NextApiJsonArgument[]>(argsString, SerializationUtils.GetJsonConfig())
                        .Cast<INextApiArgument>()
                        .ToArray();
            }

            var result = await _handler.ExecuteCommand(command);
            if (result is NextApiFileResponse fileResponse)
            {
                await context.Response.SendNextApiFileResponse(fileResponse);
                return;
            }

            if (useMessagePack)
            {
                var resultByteArray = MessagePackSerializer.Typeless.Serialize(result);
                await context.Response.SendByteArray(resultByteArray);
            }
            else
                await context.Response.SendJson(result);
        }

        /// <summary>
        /// Entry point for NextApi HTTP requests
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessHttpRequestAsync(HttpContext context)
        {
            var route = context.GetRouteData().Values;
            var service = route["service"].ToString();
            var method = route["method"].ToString();
            _userAccessor.User = context.User;

            var requestData = new Dictionary<string, object>();
            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                
                requestData = form.Keys.ToDictionary(s => s, s => JsonConvert.DeserializeObject(form[s], SerializationUtils.GetJsonConfig()));
                _request.FilesFromClient = form.Files;
            }
            else if (context.Request.ContentType == "application/json")
            {
                requestData = JsonConvert.DeserializeObject<Dictionary<string, object>>(await new StreamReader(context.Request.Body).ReadToEndAsync(), SerializationUtils.GetJsonConfig());
            }

            var command = new NextApiCommand
            {
                Service = service,
                Method = method,
                Args = requestData?.Select(s => new NextApiJsonArgument(s.Key, s.Value)).ToArray()
            };

            var result = await _handler.ExecuteCommand(command);
            if (result is NextApiFileResponse fileResponse)
            {
                await context.Response.SendNextApiFileResponse(fileResponse);
                return;
            }

            await context.Response.SendJson(result);
        }

        /// <summary>
        /// Returns list of supported permissions
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetSupportedPermissions(HttpContext context)
        {
            var permissions = _handler.GetSupportedPermissions();
            await context.Response.SendJson(permissions);
        }
    }
}
