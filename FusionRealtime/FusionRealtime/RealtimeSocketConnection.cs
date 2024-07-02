using System;

namespace MVS.Realtime
{
    #nullable disable
    public abstract class RealtimeSocketConnection
    {
        public string ConnectAddress;
        protected internal PeerBase peerBase;
        protected readonly ConnectionProtocol Protocol;
        public bool PollReceive;
        protected IRealtimePeerListener Listener => peerBase.Listener;
        public RealtimeSocketState State { get; protected set; }
        public bool Connected => State == RealtimeSocketState.Connected;
        public string ServerAddress { get; protected set; }
        public static string ServerIpAddress { get; protected set; }
        public static string ServerPort { get; protected set; }

        public RealtimeSocketConnection(PeerBase peerBase)
        {
            Protocol = peerBase != null ? peerBase.Protocol : throw new Exception("No Peer!!");
            this.peerBase = peerBase;
            this.ConnectAddress = this.peerBase.ServerAddress;
        }

        public virtual bool Connect()
        {
            if (State != RealtimeSocketState.Disconnected)
            {
                peerBase.Listener.MVSDebug(DebugLevel.ERROR, $"Connect() failed! Current State is {State.ToString()}");
                return false;
            }

            if (peerBase == null || Protocol != peerBase.Protocol)
                return false;

            RealtimeSocketConnection.ServerIpAddress = string.Empty;
            if(!TryParseAddress(peerBase.ServerAddress, out var address, out var port))
            {
                peerBase.Listener.MVSDebug(DebugLevel.ERROR, $"Failed To Parsing Address: {peerBase.ServerAddress}");
                return false;
            }

            ServerAddress = address;
            ServerPort = port;
            peerBase.Listener.MVSDebug(DebugLevel.ALL, $"Socket.Connect() {ServerAddress}:{ServerPort}, Protocol : {Protocol.ToString()}");
            return true;
        }
        
        public abstract bool Disconnect();

        public abstract bool Send(byte[] data);
        
        public abstract bool Receive(byte[] data);

        private static bool TryParseAddress(
            string url,
            out string address,
            out string port)
        {
            address = String.Empty;
            port = String.Empty;
            string str = url;
            string[] parts = str.Split(':');
            if(parts.Length == 2)
            {
                address = parts[0];
                port = parts[1];
                return true;
            }
            return false;
        }
    }
}