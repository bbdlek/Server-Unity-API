using System;
using MVS.Realtime;
using Protocol;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosObject")]
    public class HeliosObject : HeliosMonoBehavior
    {
        [SerializeField]
        private uint _prefabId;
        [SerializeField]
        private uint _instanceId;
        
        public uint PrefabId
        {
            get => _prefabId;
            set
            {
                ObjectInfo.ObjectID.PrefabID = value;
                _prefabId = value;
            }
        }

        public uint InstanceId
        {
            get => _instanceId;
            set
            {
                ObjectInfo.ObjectID.InstanceID = value;
                _instanceId = value;
            }
        }

        public HeliosTransform heliosTransform;
        
    }
}