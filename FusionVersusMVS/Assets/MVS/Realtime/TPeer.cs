namespace MVS.Realtime
{
    #nullable disable
    public class TPeer : PeerBase
    {
        private SocketTcp _socketTcp;
        protected internal bool DoFraming = true;
        
        internal override bool Connect(string serverAddress, string appId, ServerConnection serverType)
        {
            if (!realtimeSocket.Connect())
                return false;
            Listener.MVSDebug(DebugLevel.INFO, "TPeer Connect()");
            peerConnectionState = ConnectionStateValue.Connecting;
            return true;
        }

        public override void OnConnect()
        {
            _socketTcp = (SocketTcp)realtimeSocket;
        }

        internal override void Disconnect()
        {
            
        }

        internal override void StopConnection()
        {
            
        }

        internal override bool SendPacket(EventCode eventCode, byte[] data, int size)
        {
            realtimeSocket.Send(eventCode, data, size);
            return true;
        }

        internal override bool ProcessIncomingData()
        {
            _socketTcp.wsh.ProcessReceiveData();
            return true;
        }

        internal override bool ProcessOutgoingData()
        {
            return true;
        }
    }
}