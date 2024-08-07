using UnityEngine;

namespace _1_Scripts._8_HeliosTest
{
    public class TestDefault : MonoBehaviour
    {
        [SerializeField]
        private GameObject go;

        public void Test()
        {
            Debug.Log($"TestDefault: {go.name}");
        }
    }
}