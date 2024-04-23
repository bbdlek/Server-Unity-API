#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVS.Helios
{
    [CustomEditor(typeof(HeliosSettings))]
    public class HeliosSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            HeliosSettings settings = (HeliosSettings)target;

            if (GUILayout.Button("Find and Add HeliosObject Prefabs"))
            {
                AddHeliosObjectPrefabs(settings);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void AddHeliosObjectPrefabs(HeliosSettings settings)
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject");
            List<GameObject> prefabsToAdd = new List<GameObject>();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                HeliosObject heliosObject = prefabObject.GetComponent<HeliosObject>();
                if (heliosObject != null)
                {
                    prefabsToAdd.Add(prefabObject);
                }
            }

            foreach (var prefabObject in prefabsToAdd)
            {
                HeliosObject comp = prefabObject.GetComponent<HeliosObject>();
                if (comp != null)
                {
                    if(!settings.NetworkPrefabs.Prefabs.Contains(comp))
                    {
                        settings.NetworkPrefabs.Prefabs.Add(comp);
                        comp.PrefabId = (uint)settings.NetworkPrefabs.FindNetworkIdByPrefab(comp);
                        Debug.Log(comp.PrefabId);
                    }

                    EditorUtility.SetDirty(settings);
                }
            }
        }
    }
}
#endif