using System;
using System.Collections.Generic;
using Google.Protobuf;
using Protocol;
using Type = System.Type;

namespace MVS.Realtime
{
    // mvs auth tokens for join room in mvs
    public struct RoomJoinInfoStruct
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public UInt64 RoomID { get; set; }
        public string RoomName { get; set; }
        public UInt64 MvsUserID { get; set; }
        public string MvsUserToken { get; set; }   
    }
    
    public class RealtimePeer : MVSPeer
    {
        public RealtimePeer(ConnectionProtocol protocol) : base(protocol)
        {
            this.ConfigUnitySockets();
        }
        
        public RealtimePeer(IRealtimePeerListener listener, ConnectionProtocol protocol) : this(protocol)
        {
            base.Listener = listener;
            Listener.MVSDebug(DebugLevel.INFO, "RealtimePeer Created");
        }

        private void ConfigUnitySockets()
        {
            UnityEngine.Debug.Log("ConfigUnitySockets");
            Type websocketType = null;
#if UNITY_WEBGL && UNITY_EDITOR
            websocketType = Type.GetType("MVS.Realtime.MVSWebSocket, MVSWebSocket", false);
            if (websocketType == null)
            {
                websocketType = Type.GetType("MVS.Realtime.MVSWebSocket, Assembly-CSharp-firstpass", false);
            }
            if (websocketType == null)
            {
                websocketType = Type.GetType("MVS.Realtime.MVSWebSocket, Assembly-CSharp", false);
            }
#elif UNITY_WEBGL && !UNITY_EDITOR 
            if (websocketType == null)
            {
                websocketType = Type.GetType("MVS.Realtime.MVSWebGLSocket, MVSWebGLSocket", false);
                // this.Listener.MVSDebug(DebugLevel.WARNING, "SocketWebTcp type not found in the usual Assemblies. This is required as wrapper for the browser WebSocket API. Make sure to make the PhotonLibs\\WebSocket code available.");
            }
            if (websocketType == null)
            {
                websocketType = Type.GetType("MVS.Realtime.MVSWebGLSocket, Assembly-CSharp-firstpass", false);
                // this.Listener.MVSDebug(DebugLevel.WARNING, "SocketWebTcp type not found in the usual Assemblies. This is required as wrapper for the browser WebSocket API. Make sure to make the PhotonLibs\\WebSocket code available.");
            }
            if (websocketType == null)
            {
                websocketType = Type.GetType("MVS.Realtime.MVSWebGLSocket, Assembly-CSharp", false);
                // this.Listener.MVSDebug(DebugLevel.WARNING, "SocketWebTcp type not found in the usual Assemblies. This is required as wrapper for the browser WebSocket API. Make sure to make the PhotonLibs\\WebSocket code available.");
            }
#endif

            if (websocketType != null)
            {
                UnityEngine.Debug.Log("ConfigUnitySockets()" + websocketType);
                this.SocketImplementationConfig[ConnectionProtocol.WebSocket] = websocketType;
            }
        }

        public virtual bool OpRoomList()
        {
            /*var fixedData = new C_TEST_ROOM_LIST();
            return SendOperation(Protocol.OperationCode.RoomList, fixedData);*/

            return true;
        }

        public virtual bool OpCreateRoom(RoomJoinInfoStruct opParams)
        {
            var fixedData = new C_ROOM_JOIN_OR_CREATE
            {
                UserID = opParams.MvsUserID,
                MvsUserToken = opParams.MvsUserToken,
                RoomID = opParams.RoomID
            };
            return SendOperation(Protocol.OperationCode.RoomJoinOrCreate, fixedData);
        }

        public virtual bool OpGroupList()
        {
            var fixedData = new C_GROUP_LIST();
            return SendOperation(Protocol.OperationCode.GroupList, fixedData);
        }

        public virtual bool OpJoinGroup(C_GROUP_JOIN groupJoinPkt)
        {
            return SendOperation(Protocol.OperationCode.GroupJoin, groupJoinPkt);
        }

        public virtual bool OpAddNetworkObject(C_ADD_NETWORK_OBJECTS addNetworkObjectsPkt)
        {
            return SendEvent(EventCode.PKT_C_ADD_NETWORK_OBJECTS, addNetworkObjectsPkt);
        }

        /*public virtual bool OpInitVariables(C_INIT_VARIABLES initVariablesPkt)
        {
            return SendOperation(Protocol.OperationCode.InitVariables, initVariablesPkt);
        }*/

        public override bool SendEvent(int eventCode, IMessage fixedData = null, List<Protocol.HeliosVariable> customData = null)
        {
            base.SendEvent(eventCode, fixedData, customData);
            return true;
        }

    }

    public enum CustomAuthenticationType : byte
    {
        Custom = 0,
        None = byte.MaxValue
    }

    // public class AuthenticationValues
    // {
    //     private CustomAuthenticationType authType = CustomAuthenticationType.None;
    //
    //     public CustomAuthenticationType AuthType
    //     {
    //         get => authType;
    //         set => authType = value;
    //     }
    //     
    //     public object Token { get; protected internal set; }
    //     
    //     public string UserId { get; set; }
    //
    //     public AuthenticationValues()
    //     {
    //     }
    //
    //     public AuthenticationValues(string userId)
    //     {
    //         UserId = userId;
    //     }
    //
    //     public AuthenticationValues CopyTo(AuthenticationValues copy)
    //     {
    //         copy.AuthType = AuthType;
    //         copy.UserId = UserId;
    //         return copy;
    //     }
    // }
}


