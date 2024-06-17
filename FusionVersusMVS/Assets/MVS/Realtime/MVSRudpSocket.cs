

using System.Threading;
using UnityEngine.Scripting;
using ENet;

namespace MVS.Realtime
{
    
    public class MVSRudpSocket : RealtimeSocketConnection
    {
        private Host _client;
        private Peer _server;
        
        private readonly object syncer = new object();
        
        [Preserve]
        public MVSRudpSocket(PeerBase peerBase) : base(peerBase)
        {
        }

        ~MVSRudpSocket()
        {
        }

        public override bool Connect()
        {
            lock (syncer)
            {
                if (!base.Connect())
                    return false;
                State = RealtimeSocketState.Connecting;
            }
            
            new Thread(EnetLoop)
            {
                IsBackground = true
            }.Start();
            return true;
        }

        internal void EnetLoop()
        {
            Address address = new Address();
            address.SetHost(ServerAddress);
            address.Port = ushort.Parse(ServerPort);
            
            ENet.Library.Initialize();

            _client = new Host();
            _client.Create();
            _server = _client.Connect(address);
            Listener.MVSDebug(DebugLevel.INFO, $"Connect Enet Address {address.GetHost()} : {address.Port}  STATE : {_server.State}");
            
            ENet.Event enetEvent;

            while (true)
            {
                while (_client.Service(0, out enetEvent) > 0)
                {
                    switch (enetEvent.Type)
                    {
                        case ENet.EventType.Connect:
                        {
                            // connect
                            peerBase.OnConnect();
                        } break;
                        
                        case ENet.EventType.Disconnect:
                        {
                            
                        } break;
                        
                        case ENet.EventType.Receive:
                        {
                            byte[] data = new byte[enetEvent.Packet.Length];
                            enetEvent.Packet.CopyTo(data);
                            peerBase.ReceiveIncomingData(data);
                            enetEvent.Packet.Dispose();
                        } break;

                        case ENet.EventType.Timeout:
                        {
                            Listener.MVSDebug(DebugLevel.ERROR, $"ENET TIMEOUT | Peer State : {enetEvent.Peer.State}");
                        } break;
                    }
                }
                
                Thread.Sleep(1);
            }
        }

        public override bool Disconnect()
        {
            _server.Disconnect(0);
            _client.Dispose();
            Library.Deinitialize();

            return true;
        }

        public override bool Send(byte[] data)
        {
            Packet packet = default(Packet);
            packet.Create(data);
            _server.Send(0, ref packet);

            return true;
        }

        public override bool Receive(byte[] data)
        {
            return true;
        }
        
    }
}