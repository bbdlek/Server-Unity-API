using System;
using WebSocketSharp;
using WebSocket = WebSocketSharp.WebSocket;

namespace MVS.Realtime
{
    #nullable disable
    public abstract class PeerBase
    {
        internal MVSPeer mvsPeer;

        internal RealtimeSocketConnection realtimeSocket;
        
        public IRealtimePeerListener Listener => mvsPeer.Listener;

        internal ConnectionProtocol Protocol;

        internal ConnectionStateValue peerConnectionState = ConnectionStateValue.Disconnected;

        internal short peerID = -1;
        
        public virtual string PeerID => ((ushort) peerID).ToString();
        
        internal bool ApplicationIsInitialized;
        
        public string ServerAddress { get; internal set; }

        internal abstract bool Connect(
            string serverAddress,
            string appId,
            ServerConnection serverType
        );

        internal virtual void Reset()
        {
            peerConnectionState = ConnectionStateValue.Disconnected;
            ApplicationIsInitialized = false;
        }
        
        ~PeerBase()
        {
            Disconnect();
        }

        public abstract void OnConnect();

        internal void InitCallback()
        {
            if (peerConnectionState == ConnectionStateValue.Connecting)
                peerConnectionState = ConnectionStateValue.Connected;
            ApplicationIsInitialized = true;
            Listener.OnStatusChanged(StatusCode.Connect);
        }
        
        internal abstract void Disconnect();

        internal abstract void StopConnection();

        internal abstract bool SendPacket(EventCode eventCode, byte[] data, int size);

        internal abstract bool ProcessIncomingData();

        internal abstract bool ProcessOutgoingData();

    }
}