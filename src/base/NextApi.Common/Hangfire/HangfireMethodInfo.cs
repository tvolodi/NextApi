using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NextApi.Common.Hangfire
{
    public class HangfireMethodInfo
    {
        public string MethodName { get; set; }
        public string DeclaringTypeName { get; set; }
        public string DeclaringInterfaceName { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public string HangfireAttributeValue { get; set; }
        public string HangfireAttributeData { get; set; }
    }
}
