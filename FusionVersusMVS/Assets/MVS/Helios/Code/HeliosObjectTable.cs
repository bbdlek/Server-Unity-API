using System;
using System.Collections.Generic;

namespace MVS.Helios
{
    [Serializable]
    public class HeliosObjectTable
    {
        public List<HeliosObject> Prefabs;

        public HeliosObject FindPrefabByNetworkId(uint prefabId)
        {
            HeliosObject target;
            if(Prefabs[(int)prefabId] != null)
                return Prefabs[(int)prefabId];

            return null;
        }
    }
}