using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MVS.Realtime
{
    #nullable disable
    public class MVSPeer
    {
        internal PeerBase peerBase;
        
        public IRealtimePeerListener Listener { get; protected set; }

        public event Action<DisconnectedReason> OnDisconnectReason;

        public PeerState PeerState
        {
            get
            {
                return this.peerBase.peerConnectionState == ConnectionStateValue.Connected && !this.peerBase.ApplicationIsInitialized ? PeerState.InitializingApplication : (PeerState) this.peerBase.peerConnectionState;
            }
        }
        
        public ConnectionProtocol UsedProtocol => this.peerBase.Protocol;
        
        public ConnectionProtocol TransportProtocol { get; set; }

        public string PeerID => peerBase.PeerID;
        
        public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig;
        public Type SocketImplementation { get; internal set; }
        
        private Stopwatch trafficStatsStopwatch;
        private bool trafficStatsEnabled = false;

        public MVSPeer(ConnectionProtocol protocol)
        {
            TransportProtocol = protocol;
            SocketImplementationConfig = new Dictionary<ConnectionProtocol, Type>();
            SocketImplementationConfig[ConnectionProtocol.Sap] = typeof(SocketTcp);
            SocketImplementationConfig[ConnectionProtocol.Tcp] = typeof(SocketTcp);
            SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketTcp);
            SocketImplementationConfig[ConnectionProtocol.WebSocket] = typeof(SocketTcp);
            CreatePeerBase();
        }

        public virtual bool Connect(
            string serverAddress,
            string appId,
            ServerConnection serverType)
        {
            if (this.peerBase != null && this.peerBase.peerConnectionState != 0)
            {
                Listener.MVSDebug(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
                return false;
            }
            
            CreatePeerBase();
            peerBase.Reset();
            peerBase.ServerAddress = serverAddress;
            if (!SocketImplementationConfig.TryGetValue(TransportProtocol, out var type))
            {
                return false;
            }
            SocketImplementation = type;
            
            try
            {
                this.peerBase.realtimeSocket = (RealtimeSocketConnection) Activator.CreateInstance(SocketImplementation, peerBase);
            }
            catch (Exception ex)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"Connect() failed to create a RealtimeSocket instance for {ex.InnerException}");
                return false;
            }

            
            peerBase.Connect(serverAddress, appId, serverType);
            Listener.OnStatusChanged(StatusCode.Connect);
            return true;
        }

        private void CreatePeerBase()
        {
            switch (TransportProtocol)
            {
                case ConnectionProtocol.Tcp:
                case ConnectionProtocol.Sap:
                case ConnectionProtocol.WebSocket:
                    if (!(peerBase is TPeer tpeer))
                    {
                        tpeer = new TPeer();
                        peerBase = tpeer;
                    }
                    tpeer.DoFraming = TransportProtocol == ConnectionProtocol.Tcp;
                    
                    break;
                default:
                    break;
            }

            peerBase.mvsPeer = this;
            peerBase.Protocol = TransportProtocol;
        }

        public virtual void Service()
        {
            do
                ;
            while (this.ProcessIncomingData());
            do
                ;
            while (this.ProcessOutgoingData());
        }
        

        public virtual void Disconnect()
        {
            peerBase.Disconnect();
        }

        public static bool Handling(Func<byte[], bool> func, byte[] data, int len)
        {
            var res = func(data);
            return res;
        }

        public virtual bool ProcessIncomingData()
        {
            // TODO: 시간 정해서 들어온 데이터 처리
            return peerBase.ProcessIncomingData();
        }

        public virtual bool ProcessOutgoingData()
        {
            // TODO: 시간 정해서 나가는 데이터 처리
            return peerBase.ProcessOutgoingData();
        }

        public virtual bool SendEvent(EventCode eventCode, byte[] data, int size)
        {
            peerBase.SendPacket(eventCode, data, size);
            return true;
        }
        
    }
}