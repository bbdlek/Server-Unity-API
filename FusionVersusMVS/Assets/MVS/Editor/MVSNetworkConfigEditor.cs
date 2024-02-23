using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MVSNetworkConfig))]
public class MVSNetworkConfigEditor : Editor
{
    //Editor
    private bool _showAppSettings = true;
    private bool _showNetworkObjects = true;
    
    private MVSNetworkConfig comp;

    private void OnEnable()
    {
        comp = (MVSNetworkConfig)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _showAppSettings = EditorGUILayout.Foldout(_showAppSettings, "Network Config");
        if (_showAppSettings)
        {
            comp.mvsVersion = EditorGUILayout.TextField("MVS Version", comp.mvsVersion);
        }

        _showNetworkObjects = EditorGUILayout.Foldout(_showNetworkObjects, "Network Objects");
        if (_showNetworkObjects)
        {
            for (int i = 0; i < comp.NetworkObjectTable._mvsNetworkObjects.Count; i++)
            {
                EditorGUILayout.ObjectField("Network Object " + i, comp.NetworkObjectTable._mvsNetworkObjects[i], typeof(MVSNetworkObject), false);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
