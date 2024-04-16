namespace MVS.Realtime
{
    #nullable disable
    public class TPeer : PeerBase
    {
        private MVSWebSocket _mvsWebSocket;
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
            _mvsWebSocket = (MVSWebSocket)realtimeSocket;
        }

        internal override void Disconnect()
        {
            realtimeSocket.Disconnect();
            Listener.MVSDebug(DebugLevel.INFO, "TPeer Disconnect()");
            peerConnectionState = ConnectionStateValue.Disconnected;
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
            if(_mvsWebSocket != null)
            {
                if(_mvsWebSocket.PollReceive)
                    _mvsWebSocket.wsh.ProcessReceiveData();
            }
            return true;
        }

        internal override bool ProcessOutgoingData()
        {
            return true;
        }
    }
}