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
            realtimeSocket.Disconnect();
            Listener.MVSDebug(DebugLevel.INFO, "TPeer Disconnect()");
            peerConnectionState = ConnectionStateValue.Disconnecting;
        }

        internal override void StopConnection()
        {
            
        }

        internal override bool SendPacket(byte[] data, int size)
        {
            realtimeSocket.Send(data, size);
            return true;
        }

        internal override bool ProcessIncomingData()
        {
            if(_socketTcp != null)
            {
                if(_socketTcp.PollReceive)
                    _socketTcp.wsh.ProcessReceiveData();
            }
            return true;
        }

        internal override bool ProcessOutgoingData()
        {
            return true;
        }
    }
}