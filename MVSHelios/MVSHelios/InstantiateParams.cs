using MVS.Realtime;
using Protocol;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public enum InstantiateState
    {
        Success,
        Failed,
        InProgress,
        Ignore,
    }
    
    public struct InstantiateParams
    {
        public uint prefabId;
        public uint instanceId;
        public uint clientInstanceID;
        public Vector3 position;
        public Quaternion rotation;
        public Player creator;
        public InstantiateState state;
        public ObjectInfo ObjectInfo;
        
        public InstantiateParams(uint prefabId, uint instanceId, uint clientInstanceID, Vector3 position, Quaternion rotation, Player creator, InstantiateState state, ObjectInfo objectInfo)
        {
            this.prefabId = prefabId;
            this.instanceId = instanceId;
            this.clientInstanceID = clientInstanceID;
            this.position = position;
            this.rotation = rotation;
            this.creator = creator;
            this.state = state;
            ObjectInfo = objectInfo;
        }
    }
}