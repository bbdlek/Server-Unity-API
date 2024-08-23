using Protocol;
using UnityEngine;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosObject")]
    public class HeliosObject : HeliosMonoBehavior
    {
        [SerializeField]
        private uint _prefabId;
        private uint _instanceId;
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
        
        private bool _isMine = false;
        
        public new bool IsMine
        {
            get => _isMine;
            set => _isMine = value;
        }

        public HeliosTransform heliosTransform;

        private void OnDestroy()
        {
            HeliosNetwork.NetworkRemoveObject(ObjectInfo.ObjectID.InstanceID);
        }
    }
}