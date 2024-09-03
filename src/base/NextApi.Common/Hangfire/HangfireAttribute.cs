using System;
using System.Collections.Generic;
using System.Text;

namespace NextApi.Common.Hangfire
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HangfireAttribute : Attribute
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public HangfireAttribute(string name, string data = null)
        {
            Name = name;
            Data = data;
        }
    }
}
