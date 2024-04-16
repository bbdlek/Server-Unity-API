using UnityEngine;

namespace MVS.Helios
{
    public interface IHeliosObservable
    {
        void OnHeliosSerializeView();
    }

    public interface IHeliosPrefabPool
    {
        GameObject Instantiate(uint prefabId, Vector3 position, Quaternion rotation);

        void Destroy(GameObject gameObject);
    }
}