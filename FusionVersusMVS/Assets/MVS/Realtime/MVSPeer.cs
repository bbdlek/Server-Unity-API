using System;
using System.Collections.Generic;
using System.Diagnostics;
using _1_Scripts._8_HeliosTest;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;
using Type = System.Type;

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
            
            Listener.OnStatusChanged(StatusCode.Connect);
            peerBase.Connect(serverAddress, appId, serverType);
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

        public virtual bool SendEvent(int eventCode, IMessage fixedData = null, CustomDic customData = null)
        {
            var packs = new Packs();
            var pkt = new C_EVENT
            {
                EventCode = eventCode,
            };
            switch (eventCode)
            {
                case EventCode.PKT_C_INITIAL_OBJECTS:
                    packs.CInitialObjects = fixedData as C_INITIAL_OBJECTS;
                    break;
                case EventCode.PKT_C_ADD_NETWORK_OBJECTS:
                    packs.CAddNetworkObjects = fixedData as C_ADD_NETWORK_OBJECTS;
                    break;
                case EventCode.PKT_C_UPDATE_NETWORK_OBJECTS:
                    packs.CUpdateNetworkObjects = fixedData as C_UPDATE_NETWORK_OBJECTS;
                    break;
                case EventCode.PKT_C_REMOVE_NETWORK_OBJECTS:
                    packs.CRemoveNetworkObjects = fixedData as C_REMOVE_NETWORK_OBJECTS;
                    break;
                case EventCode.PKT_C_CHANGE_OBJECTS_OWNER:
                    packs.CChangeObjectsOwner = fixedData as C_CHANGE_OBJECTS_OWNER;
                    break;
                case (int)Protocol.EventCode.Rpc:
                    packs.CRpc = fixedData as C_RPC;
                    Listener.MVSDebug(DebugLevel.INFO, packs.CRpc.InstanceID.ToString());
                    break;
            }
            if(fixedData != null)
                pkt.FixedData = packs;
            
            SendOperation(Protocol.OperationCode.RaiseEvent, pkt, customData);
            return true;
        }

        public virtual bool SendOperation(
            Protocol.OperationCode operationCode,
            IMessage fixedData,
            CustomDic customData = null
        )
        {
            (byte[] data, int size) = peerBase.SerializeOperationToPacket(operationCode, fixedData, customData);
            peerBase.SendPacket(data, size);
            return true;
        }

    }
}