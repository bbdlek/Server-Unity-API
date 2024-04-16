using UnityEngine;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosObject")]
    public class HeliosObject : HeliosMonoBehavior
    {
        [SerializeField]
        private uint _prefabId;
        [SerializeField]
        private uint _instanceId;

        private HeliosMonoBehavior _views;
        
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
        
        protected bool isMine = false;
        
        public bool IsMine
        {
            get
            {
                return isMine;
            }
            set
            {
                isMine = value;
            }
        }

        public HeliosTransform heliosTransform;

        private void OnDestroy()
        {
            HeliosNetwork.NetworkRemoveObject(InstanceId);
        }
    }
}