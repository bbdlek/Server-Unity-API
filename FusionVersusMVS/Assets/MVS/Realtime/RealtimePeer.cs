using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;
using UnityEngine;
using Type = System.Type;

namespace MVS.Realtime
{
    public class RealtimePeer : MVSPeer
    {
        public RealtimePeer(ConnectionProtocol protocol) : base(protocol)
        {
            // this.ConfigUnitySockets();
        }
        
        public RealtimePeer(IRealtimePeerListener listener, ConnectionProtocol protocol) : this(protocol)
        {
            base.Listener = listener;
            Listener.MVSDebug(DebugLevel.INFO, "RealtimePeer Created");
        }

        private void ConfigUnitySockets()
        {
            Type websocketType = null;
            websocketType = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, PhotonWebSocket", false);
            if (websocketType == null)
            {
                websocketType = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp-firstpass", false);
            }
            if (websocketType == null)
            {
                websocketType = Type.GetType("ExitGames.Client.Photon.SocketWebTcp, Assembly-CSharp", false);
            }

            if (websocketType != null)
            {
                
            }
        }

        public virtual bool OpCreateRoom(JoinRoomParams opParams)
        {
            Dictionary<Parameter, object> parameter = new Dictionary<Parameter, object>();

            if (!string.IsNullOrEmpty(opParams.AuthToken))
            {
                parameter[Parameter.Authtoken] = opParams.AuthToken;
            }

            parameter[Parameter.Appid] = opParams.AppID;

            parameter[Parameter.Roomid] = opParams.RoomID;

            if (!string.IsNullOrEmpty(opParams.Name))
            {
                parameter[Parameter.Roomname] = opParams.Name;
            }
            
            return SendOperation(Protocol.OperationCode.RoomJoinOrCreate, parameter);
        }

        public virtual bool OpJoinGroup(C_GROUP_JOIN groupJoinPkt)
        {
            return SendEvent(EventCode.PKT_C_GROUP_JOIN, groupJoinPkt.ToByteArray(), 
                groupJoinPkt.CalculateSize());
        }

        public virtual bool OpAddNetworkObject(C_ADD_NETWORK_OBJECTS addNetworkObjectsPkt)
        {
            return SendEvent(EventCode.PKT_C_ADD_NETWORK_OBJECTS, addNetworkObjectsPkt.ToByteArray(),
                addNetworkObjectsPkt.CalculateSize());
        }

        public override bool SendEvent(EventCode eventCode, byte[] data, int size)
        {
            base.SendEvent(eventCode, data, size);
            return true;
        }

    }

    public class JoinRoomParams
    {
        public string AuthToken;
        public long AppID;
        public long RoomID;
        public string Name;
    }
    
    public enum AuthModeOption { None, Auth }

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


