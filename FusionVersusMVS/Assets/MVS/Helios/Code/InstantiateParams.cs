using MVS.Realtime;
using UnityEngine;

namespace MVS.Helios
{
    public struct InstantiateParams
    {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;
        public Player creator;

        public InstantiateParams(string prefabName, Vector3 position, Quaternion rotation, Player creator)
        {
            this.prefabName = prefabName;
            this.position = position;
            this.rotation = rotation;
            this.creator = creator;
        }
    }
}