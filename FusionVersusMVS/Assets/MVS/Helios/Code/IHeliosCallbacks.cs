using UnityEngine;

namespace MVS.Helios
{
    public interface IHeliosPrefabPool
    {
        GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation);

        void Destroy(GameObject gameObject);
    }
}