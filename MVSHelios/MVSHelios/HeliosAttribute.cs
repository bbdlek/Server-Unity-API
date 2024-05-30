namespace MVS.Helios
{
    using System;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class HNSyncAttribute : Attribute
    {
        public HeliosMonoBehavior Owner;
    }
}