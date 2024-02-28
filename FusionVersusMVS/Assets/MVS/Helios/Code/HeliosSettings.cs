using System;
using MVS.Realtime;
using UnityEngine;

namespace MVS.Helios
{
    
    [CreateAssetMenu(menuName = "Helios/Helios Settings", fileName = "HeliosServerSettings")]
    [Serializable]
    public class HeliosSettings : ScriptableObject
    {
        [Tooltip("Core MVS Settings")]
        public AppSettings AppSettings;
        
        // Helios Log Settings
        
        [Tooltip("Run In Background")]
        public bool RunInBackground = true;
    }
}