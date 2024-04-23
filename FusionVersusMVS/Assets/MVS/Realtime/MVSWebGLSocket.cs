using System;
using HybridWebSocket;
using UnityEngine.Scripting;

namespace MVS.Realtime
{
    public class MVSWebGLSocket : RealtimeSocketConnection, IDisposable
    {
        // private WebSocket sock;
        private HybridWebSocket.WebSocket _socket;
        bool m_IsConnected;
        string m_Error;

        private readonly object syncer = new object();
        
        [Preserve]
        public MVSWebGLSocket(PeerBase npeer) : base(npeer)
        {
            Listener.MVSDebug(DebugLevel.INFO, typeof(MVSWebGLSocket).ToString());
            ServerAddress = npeer.ServerAddress;
            
            Listener.MVSDebug(DebugLevel.INFO, "MVSWebGLSocket, .Net, Unity");
            Listener.MVSDebug(DebugLevel.INFO, peerBase.ToString());
            
            PollReceive = false;
        }

        public void Dispose()
        {
            State = RealtimeSocketState.Disconnecting;

            if (_socket != null)
            {
                try
                {
                    if (m_IsConnected)
                    {
                        _socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    Listener.MVSDebug(DebugLevel.INFO, "Exception in SocketWebTcp.Dispose(): " + ex);
                }
            }

            _socket = null;
            State = RealtimeSocketState.Disconnected;
        }


        public override bool Connect()
        {
            State = RealtimeSocketState.Connecting;
            
            try
            {
                _socket = WebSocketFactory.CreateInstance("ws://" + ConnectAddress);
                
                _socket.OnMessage += (byte[] msg) =>
                {
                    Receive(msg);
                };
                _socket.OnOpen += () =>
                {
                    m_IsConnected = true;
                    State = RealtimeSocketState.Connected;
                    peerBase.OnConnect();
                    PollReceive = true;
                };
                _socket.OnError += (string errMsg) =>
                {
                    Listener.MVSDebug(DebugLevel.ERROR, "WS error: " + errMsg);
                };
            
                _socket.OnClose += code =>
                {
                    Listener.MVSDebug(DebugLevel.INFO, "WebSocket closed with code: " + code);
                    Listener.OnStatusChanged(StatusCode.Disconnect);
                    peerBase.peerConnectionState = (ConnectionStateValue)PeerState.Disconnected;
                };

                _socket.Connect();

                return true;
            }
            catch (Exception e)
            {
                Listener.MVSDebug(DebugLevel.ERROR, "SocketWebTcp.Connect() caught exception: " + e);
                return false;
            }
        }

        public override bool Disconnect()
        {
            Listener.MVSDebug(DebugLevel.INFO, "SocketWebTcp.Disconnect()");

            State = RealtimeSocketState.Disconnecting;

            lock (syncer)
            {
                if (_socket != null)
                {
                    try
                    {
                        _socket.Close();
                    }
                    catch (Exception ex)
                    {
                        Listener.MVSDebug(DebugLevel.ERROR, "Exception in SocketWebTcp.Disconnect(): " + ex);
                    }

                    _socket = null;
                }
            }

            State = RealtimeSocketState.Disconnected;
            return true;
        }

        public override bool Send(byte[] data)
        {
            if (State != RealtimeSocketState.Connected)
            {
                
            }

            try
            {
                if (_socket != null)
                {
                    _socket.Send(data);
                    // sock.Send(data);
                }
            }
            catch (Exception e)
            {
                Listener.MVSDebug(DebugLevel.ERROR, "Cannot send to: " + ServerAddress + ". " + e.Message);
                return false;
            }

            return true;
        }

        public override bool Receive(byte[] data)
        {
            peerBase.ReceiveIncomingData(data);
            return true;
        }
    }
}