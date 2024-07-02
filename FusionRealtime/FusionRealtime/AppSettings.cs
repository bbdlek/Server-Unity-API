using System;
using UnityEngine.Serialization;

namespace MVS.Realtime
{
    /// <summary>
    /// Settings for MVS Application To Connect With MVS Server
    /// </summary>
    [Serializable]
    public class AppSettings
    {
        /// <summary> AppId for MVS Application </summary>
        public string AppId;

        /// <summary> AppVersion for MVS Application </summary>
        public string AppVersion;

        /// <summary> FixedRegion for MVS Application </summary>
        public string FixedRegion;

        /// <summary>
        /// NameServer IP for get MVM address
        /// </summary>
        public string NameServer = "";
        
        /// <summary>
        /// MV Master IP for Join MVM
        /// </summary>
        public string MVM = ""; 
        
        /// <summary> Server IP for MVS Application </summary>
        public string Server = "";

        /// <summary> Server Port for MVS Application </summary>
        public int Port = 0;

        public bool IsUsingNameServer;

        /// <summary> Connection Protocol for MVS Application </summary>
        public ConnectionProtocol Protocol = ConnectionProtocol.Tcp;

        /// <summary> Debug Lever for MVS Application </summary>
        public DebugLevel DebugLevel = DebugLevel.ERROR;

        public bool IsDefaultPort => Port <= 0;

        public AppSettings CopyTo(AppSettings s)
        {
            s.AppId = AppId;
            s.AppVersion = AppVersion;
            s.FixedRegion = FixedRegion;
            s.NameServer = NameServer;
            s.MVM = MVM;
            s.Server = Server;
            s.Port = Port;
            s.Protocol = Protocol;
            s.DebugLevel = DebugLevel;
            return s;
        }

        public AppSettings GetCopy() => CopyTo(new AppSettings());

    }
}
