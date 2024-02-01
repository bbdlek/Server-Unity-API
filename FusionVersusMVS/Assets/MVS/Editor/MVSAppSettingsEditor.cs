using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MVSAppSettings))]
public class MVSAppSettingsEditor : Editor
{
    //Editor
    private bool _showAppSettings = true;
    
    private MVSAppSettings comp;

    private void OnEnable()
    {
        comp = (MVSAppSettings)target;
    }

    public override void OnInspectorGUI()
    {
        _showAppSettings = EditorGUILayout.Foldout(_showAppSettings, "App Settings");
        if (_showAppSettings)
        {
            comp.mvsURI = EditorGUILayout.TextField("MVS URI", comp.mvsURI);
            comp.mvsPort = EditorGUILayout.TextField("MVS Port", comp.mvsPort);
        }
    }
}
