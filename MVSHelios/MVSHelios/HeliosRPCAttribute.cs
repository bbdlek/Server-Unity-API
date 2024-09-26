using System;

namespace MVS.Helios
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HeliosRPCAttribute : Attribute
    {
    }
}