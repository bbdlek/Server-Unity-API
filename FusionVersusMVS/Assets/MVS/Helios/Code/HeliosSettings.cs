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

        [Tooltip("Network Frequency")]
        [Range(1.0f, 60.0f)]
        public float SendRate = 30.0f;
        
        [Tooltip("Run In Background")]
        public bool RunInBackground = true;

        public HeliosObjectTable NetworkPrefabs = new HeliosObjectTable();

    }
}