namespace MVS.Helios
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class NetworkedAttribute : Attribute
    {
        public HeliosMonoBehavior Owner;
    }
}