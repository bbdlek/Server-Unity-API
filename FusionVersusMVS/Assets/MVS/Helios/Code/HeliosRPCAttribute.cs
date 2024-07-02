using System;

namespace MVS.Helios
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HeliosRPCAttribute : Attribute
    {
        public string Target { get; }
        public uint[] TargetPlayerIDs { get; }

        public HeliosRPCAttribute(string target, params uint[] targetPlayerIDs)
        {
            Target = target;
            TargetPlayerIDs = targetPlayerIDs;
        }
    }
}