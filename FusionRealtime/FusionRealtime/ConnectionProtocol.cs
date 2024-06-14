namespace MVS.Realtime
{
    public enum ConnectionProtocol : byte
    {
        Sap = 0,
        Udp = 1,
        Tcp = 2,
        WebSocket = 3,
        WebSocketSecure = 4
    }
}