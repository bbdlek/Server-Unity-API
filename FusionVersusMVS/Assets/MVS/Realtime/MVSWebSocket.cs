using System;
using System.Threading;
using UnityEngine.Scripting;
using WebSocketSharp;

namespace MVS.Realtime
{
    #nullable disable
    public class MVSWebSocket : RealtimeSocketConnection
    {
        private WebSocket ws;
        public WebSocketHandler wsh;
        
        private readonly object syncer = new object();
        
        [Preserve]
        public MVSWebSocket(PeerBase peerBase) : base(peerBase)
        {
            Listener.MVSDebug(DebugLevel.INFO, "SocketTcp, .Net, Unity");
            
            // 데이터 수신을 폴링하지 않음
            PollReceive = false;
        }

        // ~MVSWebSocket() => this.Dispose();
        //
        // public void Dispose()
        // {
        //     State = RealtimeSocketState.Disconnecting;
        //     if (ws != null)
        //     {
        //         try
        //         {
        //             if (ws.IsAlive)
        //                 ws.Close();
        //         }
        //         catch (Exception e)
        //         {
        //             Listener.MVSDebug(DebugLevel.INFO, $"Exception in Dispose : {e?.ToString()}");
        //         }
        //     }
        //
        //     ws = null;
        //     State = RealtimeSocketState.Disconnected;
        // }

        public override bool Connect()
        {
            lock (syncer)
            {
                if (!base.Connect())
                    return false;
                State = RealtimeSocketState.Connecting;
            }
            new Thread(new ThreadStart(DnsAndConnect))
            {
                IsBackground = true
            }.Start();
            return true;
        }

        public override bool Disconnect()
        {
            Listener.MVSDebug(DebugLevel.INFO, "SocketTCP.Disconnect()");
            lock (syncer)
            {
                State = RealtimeSocketState.Disconnecting;
                if (ws != null)
                {
                    try
                    {
                        if (ws.IsAlive)
                            ws.Close();
                    }
                    catch (Exception e)
                    {
                        Listener.MVSDebug(DebugLevel.INFO, $"Exception in Dispose : {e?.ToString()}");
                    }
                }

                State = RealtimeSocketState.Disconnected;
            }

            return true;
        }

        public override bool Send(byte[] data, int size)
        {
            if (wsh == null)
            {
                wsh = new WebSocketHandler(this);
                wsh.Init();
            }
            wsh.SendPacket(WebSocketHandler.PKT_ID.PKT_C_OPERATION, data, size);
            return true;
        }

        public void SendPacket(byte[] data)
        {
            ws.Send(data);
        }

        public override bool Receive(EventCode eventCode, byte[] data, int size)
        {
            return true;
        }

        internal void DnsAndConnect()
        {
            ws = new WebSocket($"ws://{ServerAddress}:{ServerPort}");

            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnWebSocketMessage;
            ws.OnClose += OnWebSocketClose;
            
            try
            {
                ws.ConnectAsync();
            }
            catch (Exception ec)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"Web Socket Connection Fail : {ec}");
                throw;
            }
            
        }
        
        private void OnWebSocketOpen(object sender, System.EventArgs e)
        {
            Listener.MVSDebug(DebugLevel.INFO, "WebSocket connected");
            State = RealtimeSocketState.Connected;
            peerBase.OnConnect();
            //Todo : TPeer로 옮겨야 하나
            wsh = new WebSocketHandler(this);
            wsh.Init();
            PollReceive = true;
        }

        // run in worker thread
        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            try
            {
                wsh.OnReceiveData(e.RawData);
            }
            catch (Exception exception)
            {
                Listener.MVSDebug(DebugLevel.ERROR, exception.ToString());
                throw;
            }
        }

        // run in worker thread
        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            Listener.MVSDebug(DebugLevel.INFO, "WebSocket closed with code: " + e.Reason);
            Listener.OnStatusChanged(StatusCode.Disconnect);
            peerBase.peerConnectionState = (ConnectionStateValue)PeerState.Disconnected;
        }
    }
}