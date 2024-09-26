using System;
using System.Collections.Generic;
using MVS.Realtime;

namespace MVS.Helios
{
    [Serializable]
    public class HeliosObjectTable
    {
        [ReadOnly]
        public List<HeliosObject> Prefabs;

        public HeliosObject FindPrefabByNetworkId(uint prefabId)
        {
            if (Prefabs.Find(x => x.PrefabId == prefabId) != null)
                return Prefabs.Find(x => x.PrefabId == prefabId);

            return null;
        }

        public int FindNetworkIdByPrefab(HeliosObject ho)
        {
            return Prefabs.LastIndexOf(ho);
        }
    }
}