using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Bootstrap))]
public class BootstrapEditor : Editor
{
    //Tab Group
    private int _tabIndex;
    private readonly string[] _tabGroupNames = { "Fusion", "MVS" };
    
    private Bootstrap _comp;

    private void OnEnable()
    {
        _comp = (Bootstrap)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        _tabIndex = GUILayout.Toolbar(_tabIndex, _tabGroupNames);

        switch (_tabIndex)
        {
            case 0:
                if (GUILayout.Button("Fusion Start Game"))
                {
                    _comp.fusionStarter.StartGame("Test Room");
                }
                break;
            case 1:
                if (GUILayout.Button("MVS Start Game"))
                {
                    _comp.mvsStarter.StartGame();
                }
                if (GUILayout.Button("Send Chat Test"))
                {
                    _comp.mvsStarter.SendChatTest();
                }
                break;
        }
    }
}
