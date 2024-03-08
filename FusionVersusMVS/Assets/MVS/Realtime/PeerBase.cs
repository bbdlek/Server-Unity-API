using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;
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

        internal (byte[], int) SerializeOperationToPacket(
            Protocol.OperationCode operationCode,
            Dictionary<Parameter, object> parameters)
        {
            var pkt = new C_OPERATION
            {
                OperationCode = operationCode
            };
            foreach (var pair in parameters)
            {
                switch (pair.Key)
                {
                    case Parameter.Authtoken:
                        pkt.DataDic[(int)Parameter.Authtoken] = Any.Pack(new StringValue { Value = (string)pair.Value });
                        break;
                    case Parameter.Appid:
                        pkt.DataDic[(int)Parameter.Appid] = Any.Pack(new Int64Value() { Value = (long)pair.Value });
                        break;
                    case Parameter.Roomid:
                        pkt.DataDic[(int)Parameter.Roomid] = Any.Pack(new Int64Value() { Value = (long)pair.Value });
                        break;
                    case Parameter.Roomname:
                        pkt.DataDic[(int)Parameter.Roomname] = Any.Pack(new StringValue() { Value = (string)pair.Value });
                        break;
                    case Parameter.Roominfo:
                        pkt.DataDic[(int)Parameter.Roominfo] = Any.Pack((RoomInfo)pair.Value);
                        break;
                    case Parameter.Playerid:
                        pkt.DataDic[(int)Parameter.Playerid] = Any.Pack(new UInt64Value() { Value = (ulong)pair.Value });
                        break;
                    case Parameter.Groupinfo:
                        pkt.DataDic[(int)Parameter.Groupinfo] = Any.Pack((GroupInfo)pair.Value);
                        break;
                    case Parameter.Groupid:
                        pkt.DataDic[(int)Parameter.Groupid] = Any.Pack((GroupID)pair.Value);
                        break;
                    case Parameter.Objectinfo:
                        pkt.DataDic[(int)Parameter.Objectinfo] = Any.Pack((ObjectInfo)pair.Value);
                        break;
                    case Parameter.Eventcode:
                        pkt.DataDic[(int)Parameter.Eventcode] = Any.Pack(new Int32Value() { Value = (int)pair.Value });
                        break;
                    case Parameter.Operationcode:
                        pkt.DataDic[(int)Parameter.Operationcode] = Any.Pack(new Int32Value() { Value = (int)pair.Value });
                        break;
                    case Parameter.Customstruct:
                        break;
                }
            }

            return (pkt.ToByteArray(), pkt.CalculateSize());
        }

    }
}