

using System.Threading;
using UnityEngine.Scripting;
using ENet;

namespace MVS.Realtime
{
    
    public class MVSSocketRudp : RealtimeSocketConnection
    {
        private Host _client;
        private Peer _server;
        
        private readonly object syncer = new object();
        
        [Preserve]
        public MVSSocketRudp(PeerBase peerBase) : base(peerBase)
        {
            Library.Initialize();

            _client = new Host();
            
        }

        ~MVSSocketRudp()
        {
            Library.Deinitialize();
        }

        public override bool Connect()
        {
            lock (syncer)
            {
                if (!base.Connect())
                    return false;
                State = RealtimeSocketState.Connecting;
            }
            // new Thread(DnsAndConnect)
            // {
            //     IsBackground = true
            // }.Start();
            return true;
        }

        internal void ConnectToServer()
        {
            
        }

        public override bool Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public override bool Send(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public override bool Receive(byte[] data)
        {
            throw new System.NotImplementedException();
        }
        
    }
}