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