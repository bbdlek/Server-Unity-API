namespace MVS.Realtime
{
    public enum OperationCode
    {
        Authenticate = 0,
        CreateRoom = 1,
        JoinRoom = 2,
        CreateGroup = 3,
        JoinGroup = 4,
        
        //
        GetRoomList = 10,
        
        //
        RaiseEvent = 20,

    }
}