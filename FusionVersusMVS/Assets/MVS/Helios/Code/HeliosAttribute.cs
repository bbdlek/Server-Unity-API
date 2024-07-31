namespace MVS.Helios
{
    using System;

    [AttributeUsage(AttributeTargets.Struct |AttributeTargets.Field)]
    public class HNSyncAttribute : Attribute
    {
        public HeliosMonoBehavior Owner;
    }
    
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field)]
    public class OnChangedAttribute : Attribute
    {
        public string MethodName { get; }

        public OnChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}