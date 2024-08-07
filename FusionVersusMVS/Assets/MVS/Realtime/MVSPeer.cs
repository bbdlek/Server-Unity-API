using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google.Protobuf;
using Protocol;
using Type = System.Type;

namespace MVS.Realtime
{
    public class MVSPeer
    {
        internal PeerBase peerBase;
        
        public IRealtimePeerListener Listener { get; protected set; }

        public event Action<DisconnectedReason> OnDisconnectReason;

        public PeerState PeerState
        {
            get
            {
                return peerBase.peerConnectionState == ConnectionStateValue.Connected && !peerBase.ApplicationIsInitialized ? PeerState.InitializingApplication : (PeerState) peerBase.peerConnectionState;
            }
        }

        public ConnectionProtocol TransportProtocol { get; set; }

        public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig;
        public Type SocketImplementation { get; internal set; }
        
        private Stopwatch _trafficStatsStopwatch;

        public MVSPeer(ConnectionProtocol protocol)
        {
            TransportProtocol = protocol;
            SocketImplementationConfig = new Dictionary<ConnectionProtocol, Type>();
            SocketImplementationConfig[ConnectionProtocol.Sap] = typeof(MVSWebSocket);
            SocketImplementationConfig[ConnectionProtocol.Tcp] = typeof(MVSTcpSocket);
            SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(MVSRudpSocket);
#if UNITY_WEBGL && !UNITY_EDITOR
            SocketImplementationConfig[ConnectionProtocol.WebSocket] = typeof(MVSWebGLSocket);
            SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = typeof(MVSWebGLSocket);
#else
            SocketImplementationConfig[ConnectionProtocol.WebSocket] = typeof(MVSWebSocket);
            SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = typeof(MVSWebSocket);
#endif
            CreatePeerBase();
        }

        public virtual bool Connect(
            string serverAddress,
            string appId,
            ServerConnection serverType)
        {
            if (peerBase != null && peerBase.peerConnectionState != 0)
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
                if (SocketImplementation == null)
                {
                    throw new ArgumentNullException(nameof(SocketImplementation), "SocketImplementation cannot be null.");
                }

                if (peerBase == null)
                {
                    throw new ArgumentNullException(nameof(peerBase), "peerBase cannot be null.");
                }
                
                peerBase.Listener.MVSDebug(DebugLevel.INFO, $"SocketImplementation Type: {SocketImplementation}");
                peerBase.Listener.MVSDebug(DebugLevel.INFO, $"peerBase Type: {peerBase.GetType()}");

                peerBase.realtimeSocket = (RealtimeSocketConnection)Activator.CreateInstance(SocketImplementation, peerBase);
            }
            catch (Exception ex)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"Connect() failed to create a RealtimeSocket instance for {ex.InnerException}");
                return false;
            }
            
            peerBase.Connect(serverAddress, appId, serverType);
            return true;
        }

        public void OnConnected()
        {
            Listener.OnStatusChanged(StatusCode.Connect);
        }

        private void CreatePeerBase()
        {
            switch (TransportProtocol)
            {
                case ConnectionProtocol.Tcp:
                case ConnectionProtocol.Udp:
                case ConnectionProtocol.Sap:
                case ConnectionProtocol.WebSocket:
                case ConnectionProtocol.WebSocketSecure:
                    if (!(peerBase is TPeer tpeer))
                    {
                        tpeer = new TPeer();
                        peerBase = tpeer;
                    }
                    tpeer.DoFraming = TransportProtocol == ConnectionProtocol.Tcp;
                    
                    break;
            }

            peerBase.mvsPeer = this;
            peerBase.Protocol = TransportProtocol;
        }

        public void Service()
        {
            do
            {
            } while (ProcessIncomingData());
            do
            {
            } while (ProcessOutgoingData());
        }
        

        public virtual void Disconnect()
        {
            // Listener.OnStatusChanged(StatusCode.Disconnect);
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

        public virtual bool SendEvent(int eventCode, IMessage fixedData = null, List<HeliosVariable> customData = null)
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
                    //RTT
                    StopwatchList.Add(SequenceNum, new Stopwatch());
                    StopwatchList[SequenceNum].Start();
                    if (customData == null)
                    {
                        customData = new List<HeliosVariable>();
                    }
                    customData.Add(new HeliosVariable
                    {
                        Key = 10000,
                        NInt32 = SequenceNum,
                    });
                    SequenceNum++;
                    break;
                case EventCode.PKT_C_REMOVE_NETWORK_OBJECTS:
                    packs.CRemoveNetworkObjects = fixedData as C_REMOVE_NETWORK_OBJECTS;
                    break;
                case (int)Protocol.EventCode.Rpc:
                    packs.CRpc = fixedData as C_RPC;
                    break;
            }
            if(fixedData != null)
                pkt.FixedData = packs;

            if (customData != null)
            {
                foreach (var data in customData)
                {
                    pkt.CustomData.Add(data);
                }
            }
            
            SendOperation(Protocol.OperationCode.RaiseEvent, pkt);
            return true;
        }
        
        public static int SequenceNum;
        public Dictionary<int, Stopwatch> StopwatchList = new Dictionary<int, Stopwatch>();

        public virtual bool SendOperation(
            Protocol.OperationCode operationCode,
            IMessage fixedData,
            List<HeliosVariable> customData = null
        )
        {
            (byte[] data, int size) = peerBase.SerializeOperationToPacket(operationCode, fixedData, customData);
            peerBase.SendPacket(data, size);
            return true;
        }

    }
}