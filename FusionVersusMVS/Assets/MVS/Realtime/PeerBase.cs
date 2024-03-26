using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;
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

        internal abstract bool SendPacket(byte[] data, int size);

        internal abstract bool ProcessIncomingData();

        internal abstract bool ProcessOutgoingData();

        internal (byte[], int) SerializeOperationToPacket(
            Protocol.OperationCode operationCode,
            IMessage fixedData,
            CustomDic customData = null)
        {
            var pkt = new C_OPERATION
            {
                OperationCode = operationCode,
            };
            if(fixedData != null)
            {
                pkt.FixedData = fixedData switch
                {
                    C_HEART_BEAT data => new Packs { CHeartBeat = data },
                    C_ROOM_JOIN_OR_CREATE data => new Packs { CRoomJoinOrCreate = data },
                    C_TEST_ROOM_LIST data => new Packs { CRoomList = data },
                    C_PLAYER_ID data => new Packs { CPlayerId = data },
                    C_GROUP_LIST data => new Packs { CGroupList = data },
                    C_GROUP_JOIN data => new Packs { CGroupJoin = data },
                    C_EVENT data => new Packs { CEvent = data },
                    _ => pkt.FixedData
                };
            }

            if (customData != null)
                pkt.CustomData = customData;

            return (pkt.ToByteArray(), pkt.CalculateSize());
        }

    }
}