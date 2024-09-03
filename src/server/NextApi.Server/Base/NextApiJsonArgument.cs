using Newtonsoft.Json.Linq;
using NextApi.Common;

namespace NextApi.Server.Base
{
    /// <summary>
    /// Wrapper for regular NextApiArgument usable in JSON conversion
    /// </summary>
    public class NextApiJsonArgument : INamedNextApiArgument
    {
        /// <summary>
        /// ctor
        /// </summary>
        public NextApiJsonArgument(string name, object value)
        {
            Name = name;
            Value = value is null ? JValue.CreateNull() : JToken.FromObject(value);
        }

        /// <summary>
        /// Argument value
        /// </summary>
        public JToken Value { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}
