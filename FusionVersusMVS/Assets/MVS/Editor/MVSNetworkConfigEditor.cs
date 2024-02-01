using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MVSNetworkConfig))]
public class MVSNetworkConfigEditor : Editor
{
    //Editor
    private bool _showAppSettings = true;
    
    private MVSNetworkConfig comp;

    private void OnEnable()
    {
        comp = (MVSNetworkConfig)target;
    }

    public override void OnInspectorGUI()
    {
        _showAppSettings = EditorGUILayout.Foldout(_showAppSettings, "Network Config");
        if (_showAppSettings)
        {
            comp.mvsVersion = EditorGUILayout.TextField("MVS Version", comp.mvsVersion);
        }
    }
}
