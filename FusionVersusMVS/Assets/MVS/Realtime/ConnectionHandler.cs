using System;
using UnityEngine;

namespace MVS.Realtime
{
    public class ConnectionHandler : MonoBehaviour
    {
        public bool ApplyDontDestroyOnLoad = true;
        
        [NonSerialized]
        public static bool AppQuits;
        [NonSerialized]
        public static bool AppPause;
        [NonSerialized]
        public static bool AppPauseRecent;
        [NonSerialized]
        public static bool AppOutOfFocus;
        [NonSerialized]
        public static bool AppOutOfFocusRecent;
        
        
        public RealtimeClient Client { get; set; }
        
        protected virtual void Awake()
        {
            if (this.ApplyDontDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticReset()
        {
            AppQuits = false;
            AppPause = false;
            AppPauseRecent = false;
            AppOutOfFocus = false;
            AppOutOfFocusRecent = false;
        }
        
        protected virtual void OnDisable()
        {

            if (AppQuits)
            {
                if (this.Client != null && this.Client.IsConnected)
                {
                    this.Client.Disconnect();
                }
            }
        }


        /// <summary>Called by Unity when the application gets closed. The UnityEngine will also call OnDisable, which disconnects.</summary>
        public void OnApplicationQuit()
        {
            AppQuits = true;
        }
    }
}