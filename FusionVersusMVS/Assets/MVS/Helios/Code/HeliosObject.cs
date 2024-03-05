using System;
using MVS.Realtime;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosObject")]
    public class HeliosObject : HeliosMonoBehavior
    {
        [SerializeField, ReadOnly]
        public uint prefabId;
        public uint instanceId;

        public HeliosTransform heliosTransform;

        private void Awake()
        {
            heliosTransform = GetComponent<HeliosTransform>();
        }
    }
}