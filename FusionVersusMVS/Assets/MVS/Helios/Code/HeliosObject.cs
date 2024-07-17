using Protocol;
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
        [SerializeField]
        private uint _clientInstanceId;
        
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
        
        public uint ClientInstanceId
        {
            get => _clientInstanceId;
            set
            {
                ObjectInfo.ObjectID.ClientInstanceID = value;
                _clientInstanceId = value;
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
            if(IsMine)
                HeliosNetwork.NetworkRemoveObject(ObjectInfo.ObjectID.InstanceID);
        }
    }
}