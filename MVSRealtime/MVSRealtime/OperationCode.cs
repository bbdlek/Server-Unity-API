namespace MVS.Realtime
{
    public enum OperationCode
    {
        HEART_BEAT = Protocol.OperationCode.HeartBeat,
        ROOM_JOIN_OR_CREATE = 1,
        ROOM_LEAVE = 2,
        OTHER_CLIENT_ROOM_JOINED = 3,
        OTHER_CLIENT_ROOM_LEAVE = 4,
        GROUP_LIST = 5,
        GROUP_JOIN = 6,
        RAISE_EVENT = 7,
        AUTHENTICATE = 100,
    }
}