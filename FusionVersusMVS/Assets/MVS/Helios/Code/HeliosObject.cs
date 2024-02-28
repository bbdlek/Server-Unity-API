using UnityEditor;
using UnityEngine;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosObject")]
    public class HeliosObject : MonoBehaviour
    {
        // TODO : Execution Order?
        [InitializeOnLoadMethod]
        private static void SetExecutionOrder()
        {
            int executionOrder = -16000;
            GameObject go = new GameObject();
            HeliosObject ho = go.AddComponent<HeliosObject>();
            MonoScript monoScript = MonoScript.FromMonoBehaviour(ho);

            if (executionOrder != MonoImporter.GetExecutionOrder(monoScript))
            {
                MonoImporter.SetExecutionOrder(monoScript, executionOrder);
            }

            DestroyImmediate(go);
        }
        
    }
}