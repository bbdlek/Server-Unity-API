namespace MVS.Helios
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class HNSyncAttribute : Attribute
    {
        public HeliosMonoBehavior Owner;
    }
}