using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NextApi.Common.Hangfire
{
    public static class HelperHangfire
    {
        public static HangfireMethodInfo[] GetMethods()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            HangfireMethodInfo[] methods = assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(HangfireAttribute), false).Any())
                .Select(m =>
                {
                    var declaringInterface = m.DeclaringType.GetInterfaces()
                        .FirstOrDefault(i => i.GetMethods().Any(im => im.Name == m.Name));
                    return new HangfireMethodInfo
                    {
                        MethodName = m.Name,
                        DeclaringTypeName = m.DeclaringType.FullName,
                        DeclaringInterfaceName = declaringInterface?.FullName,
                        Parameters = m.GetParameters().Select(p => new ParameterInfo { ParameterName = p.Name, ParameterType = p.ParameterType.FullName }).ToArray(),
                        HangfireAttributeValue = ((HangfireAttribute)m.GetCustomAttributes(typeof(HangfireAttribute), false).First()).Name,
                        HangfireAttributeData = ((HangfireAttribute)m.GetCustomAttributes(typeof(HangfireAttribute), false).First()).Data
                    };
                })
                .ToArray();
            return methods;
        }

        public static HangfireMethodInfo GetInformationBy(string service, string method)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            var serviceType = currentAssembly.GetTypes().FirstOrDefault(t => t.Name == service);
            if (serviceType != null)
            {
                var methodInfo = serviceType.GetMethods()
                    .FirstOrDefault(m => m.Name == method);

                if (methodInfo != null)
                {
                    var declaringInterface = methodInfo.DeclaringType.GetInterfaces()
                        .FirstOrDefault(i => i.GetMethods().Any(im => im.Name == methodInfo.Name));

                    var hangfireMethodInfo = new HangfireMethodInfo
                    {
                        MethodName = methodInfo.Name,
                        DeclaringTypeName = methodInfo.DeclaringType.FullName,
                        DeclaringInterfaceName = declaringInterface?.FullName,
                        Parameters = methodInfo.GetParameters().Select(p => new ParameterInfo { ParameterName = p.Name, ParameterType = p.ParameterType.FullName }).ToArray(),
                    };

                    return hangfireMethodInfo;
                }
            }

            return null;
        }
    }
}
