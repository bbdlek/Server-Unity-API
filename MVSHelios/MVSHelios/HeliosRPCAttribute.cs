using System;

namespace MVS.Helios
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HeliosRPCAttribute : Attribute
    {
        public string Target { get; }

        public HeliosRPCAttribute(string target)
        {
            Target = target;
        }
        
    }
}