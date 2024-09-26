namespace MVS.Realtime
{
    public class EventCode {
        public const int PKT_C_INITIAL_OBJECTS = (int)Protocol.EventCode.InitialObjects;
        public const int PKT_S_INITIAL_OBJECTS = (int)Protocol.EventCode.InitialObjects;
        public const int PKT_S_OTHER_CLIENT_JOINED = (int)Protocol.EventCode.OtherClientGroupJoined;
        public const int PKT_S_OTHER_CLIENT_LEAVE = (int)Protocol.EventCode.OtherClientGroupLeave;
        public const int PKT_C_ADD_NETWORK_OBJECTS = (int)Protocol.EventCode.AddNetworkObjects;
        public const int PKT_S_ADD_NETWORK_OBJECTS = (int)Protocol.EventCode.AddNetworkObjects;
        public const int PKT_C_REMOVE_NETWORK_OBJECTS = (int)Protocol.EventCode.RemoveNetworkObjects;
        public const int PKT_S_REMOVE_NETWORK_OBJECTS = (int)Protocol.EventCode.RemoveNetworkObjects;
        public const int PKT_C_UPDATE_NETWORK_OBJECTS = (int)Protocol.EventCode.UpdateNetworkObjects;
        public const int PKT_S_UPDATE_NETWORK_OBJECTS = (int)Protocol.EventCode.UpdateNetworkObjects;
        public const int PKT_S_CHANGE_GROUP_OWNER = (int)Protocol.EventCode.ChangeGroupOwner;
    }
}