using System;
using System.Collections;
using HybridWebSocket;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace MVS.Realtime
{
    public sealed class WaitForRealSeconds : CustomYieldInstruction
    {
        private readonly float _endTime;

        public override bool keepWaiting
        {
            get { return _endTime > Time.realtimeSinceStartup; }
        }

        public WaitForRealSeconds(float seconds)
        {
            _endTime = Time.realtimeSinceStartup + seconds;
        }
    }
    
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
        
        GameObject websocketConnectionObject;

        public override bool Connect()
        {
            State = RealtimeSocketState.Connecting;
            
            if (websocketConnectionObject != null)
            {
                Object.Destroy(websocketConnectionObject);
            }

            websocketConnectionObject = new GameObject("websocketConnectionObject");
            MonoBehaviour mb = websocketConnectionObject.AddComponent<MonoBehaviourExt>();
            websocketConnectionObject.hideFlags = HideFlags.HideInHierarchy;
            Object.DontDestroyOnLoad(websocketConnectionObject);
            
            try
            {
                Listener.MVSDebug(DebugLevel.INFO, ConnectAddress);
                _socket = WebSocketFactory.CreateInstance("ws://" + ConnectAddress);
                // sock = new WebSocket("ws://" + ConnectAddress);
                // sock.DebugReturn = (l, s) =>
                // {
                //     if (State != RealtimeSocketState.Disconnected)
                //     {
                //         Listener.MVSDebug(l, State + " " + s);
                //     }
                // };
                
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
                    Debug.Log("WS error: " + errMsg);
                };
            
                _socket.OnClose += code =>
                {
                    Debug.Log(code.ToString());
                    Listener.MVSDebug(DebugLevel.INFO, "WebSocket closed with code: " + code);
                    Listener.OnStatusChanged(StatusCode.Disconnect);
                    peerBase.peerConnectionState = (ConnectionStateValue)PeerState.Disconnected;
                };

                _socket.Connect();
                // sock.Connect();
                // mb.StartCoroutine(ReceiveLoop());

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

            if (websocketConnectionObject != null)
            {
                Object.Destroy(websocketConnectionObject);
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
        
        public IEnumerator ReceiveLoop()
        {
            Listener.MVSDebug(DebugLevel.INFO, "ReceiveLoop()");
            if (_socket != null)
            {
                while (_socket != null && !m_IsConnected && m_Error == null)
                {
                    yield return new WaitForRealSeconds(0.1f);
                }

                if (_socket != null)
                {
                    if (m_Error != null)
                    {
                        Listener.MVSDebug(DebugLevel.ERROR, "Exiting receive thread. Server: " + ServerAddress + " Error: " + m_Error);
                    }
                    else
                    {
                        // connected
                        Listener.MVSDebug(DebugLevel.ALL, "Receiving by websocket. this.State: " + State);

                        State = RealtimeSocketState.Connected;
                        
                        Listener.MVSDebug(DebugLevel.ALL, "Receiving by websocket. this.State: " + State);
                        peerBase.OnConnect();

                        while (State == RealtimeSocketState.Connected)
                        {
                            if (_socket != null)
                            {
                                if (m_Error != null)
                                {
                                    Listener.MVSDebug(DebugLevel.ERROR, "Exiting receive thread (inside loop). Server: " + ServerAddress + " Error: " + m_Error);
                                    break;
                                }

                                // byte[] inBuff = sock.Recv();
                                // if (inBuff == null)
                                // {
                                //     // nothing received. wait a bit, try again
                                //     yield return new WaitForRealSeconds(0.02f);
                                //     continue;
                                // }
                                // else
                                // {
                                //     Listener.MVSDebug(DebugLevel.INFO, "rrrrr: " + inBuff);
                                //     peerBase.ReceiveIncomingData(inBuff);
                                // }
                            }
                        }
                    }
                }
            }

            Disconnect();
        }
        
        private class MonoBehaviourExt : MonoBehaviour
        {
        }
    }
}