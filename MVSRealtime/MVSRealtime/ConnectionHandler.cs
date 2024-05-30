#if UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
#define SUPPORTED_UNITY
#endif

namespace MVS.Realtime
{
    using System;
    
#if SUPPORTED_UNITY
    using UnityEngine;
#endif

#if SUPPORTED_UNITY
    public class ConnectionHandler : MonoBehaviour
#else
    public class ConnectionHandler
#endif
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
        
#if SUPPORTED_UNITY

#if UNITY_2019_4_OR_NEWER
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticReset()
        {
            AppQuits = false;
            AppPause = false;
            AppPauseRecent = false;
            AppOutOfFocus = false;
            AppOutOfFocusRecent = false;
        }
#endif
        
        protected virtual void Awake()
        {
            if (this.ApplyDontDestroyOnLoad)
            {
                DontDestroyOnLoad(this.gameObject);
            }
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
#endif
    }
}