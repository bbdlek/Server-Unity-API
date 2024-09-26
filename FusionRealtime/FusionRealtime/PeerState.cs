namespace MVS.Realtime
{
    public enum PeerState : byte
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 3,
        Disconnecting = 4,
        InitializingApplication = 10, // 0x0A
    }
}