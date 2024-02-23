using System;

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

        /// <summary> Server IP for MVS Application </summary>
        public string Server = "222.122.186.49";

        /// <summary> Server Port for MVS Application </summary>
        public int Port;

        public bool IsUseNameServer;

        public bool IsDirectToMVS;

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
            s.Server = Server;
            s.Port = Port;
            s.Protocol = Protocol;
            s.DebugLevel = DebugLevel;
            return s;
        }

        public AppSettings GetCopy() => CopyTo(new AppSettings());

    }
}
