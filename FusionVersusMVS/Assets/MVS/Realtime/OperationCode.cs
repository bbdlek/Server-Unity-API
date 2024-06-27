namespace MVS.Realtime
{
    public enum OperationCode
    {
        HEART_BEAT = Protocol.OperationCode.HeartBeat,
        ROOM_JOIN_OR_CREATE = 1,
        ROOM_LEAVE = 2,
        ROOM_LIST = 3,
        PLAYER_ID = 4,
        GROUP_LIST = 5,
        GROUP_JOIN = 6,
        GROUP_LEAVE = 7,
        RAISE_EVENT = 8,
        INIT_VARIABLES = 9,
        AUTHENTICATE = 100,
    }
}