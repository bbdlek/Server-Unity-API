using System;
using UnityEditor;
using UnityEngine;

namespace MVS.Editor
{
    public class MVSMenu : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        [MenuItem("Tools/MVS/App Settings")]
        public static void PingAppSettings()
        {
            if(MVSAppSettings.Global != null)
            {
                EditorGUIUtility.PingObject(MVSAppSettings.Global);
                Selection.activeObject = MVSAppSettings.Global;
            }
        }
        
        [MenuItem("Tools/MVS/Network Config")]
        public static void PingNetworkConfig()
        {
            if(MVSAppSettings.Global != null)
            {
                EditorGUIUtility.PingObject(MVSNetworkConfig.Global);
                Selection.activeObject = MVSNetworkConfig.Global;
            }
        }
    }
}
