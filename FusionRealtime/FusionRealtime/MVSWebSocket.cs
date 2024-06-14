using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine.Scripting;
using WebSocketSharp;

namespace MVS.Realtime
{
    #nullable disable
    public class MVSWebSocket : RealtimeSocketConnection
    {
        private WebSocketSharp.WebSocket ws;
        private readonly X509Certificate2 trustedCertificate;
        
        private readonly object syncer = new object();
        
        [Preserve]
        public MVSWebSocket(PeerBase peerBase) : base(peerBase)
        {
            Listener.MVSDebug(DebugLevel.INFO, "MVSWebSocket, .Net, Unity");
            
            // 데이터 수신을 폴링하지 않음
            PollReceive = false;

            // string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // string certificatePath = Path.Combine(baseDirectory, "/certs/mvs.local.crt");
            //
            // if (string.IsNullOrEmpty(certificatePath) || !File.Exists(certificatePath))
            // {
            //     throw new ArgumentException("Invalid certificate path", nameof(certificatePath));
            // }
            //
            // trustedCertificate = new X509Certificate2(certificatePath);
        }

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

        public override bool Send(byte[] data)
        {
            if(State == RealtimeSocketState.Connected)
                ws.Send(data);
            return true;
        }

        public override bool Receive(byte[] data)
        {
            peerBase.ReceiveIncomingData(data);
            return true;
        }

        internal void DnsAndConnect()
        {
            string protocol = "ws";
            if (peerBase.Protocol == ConnectionProtocol.WebSocketSecure)
            {
                protocol = "wss";
            }

            string url = $"{protocol}://{ServerAddress}:{ServerPort}";
            Listener.MVSDebug(DebugLevel.INFO, url);
            ws = new WebSocketSharp.WebSocket(url);

            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnWebSocketMessage;
            ws.OnClose += OnWebSocketClose;

            if (protocol == "wss")
            {
                Listener.MVSDebug(DebugLevel.INFO, "WSS Connected");
                ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            
            try
            {
                ws.ConnectAsync();
            }
            catch (Exception ec)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"Web Socket Connection Fail : {ec}");
                HandleConnectionFailure(ec);
            }
            
        }
        
        // private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        // {
        //     if (sslPolicyErrors == SslPolicyErrors.None)
        //         return true;
        //
        //     X509Certificate2 serverCertificate = new X509Certificate2(certificate);
        //     return serverCertificate.Thumbprint == trustedCertificate.Thumbprint;
        // }
        
        private void HandleConnectionFailure(Exception ex)
        {
            State = RealtimeSocketState.Disconnected;
            Listener.OnStatusChanged(StatusCode.Disconnect);
            Listener.MVSDebug(DebugLevel.ERROR, $"Connection failure handled: {ex}");
        }
        
        private void OnWebSocketOpen(object sender, System.EventArgs e)
        {
            Listener.MVSDebug(DebugLevel.INFO, "WebSocket connected");
            State = RealtimeSocketState.Connected;
            peerBase.OnConnect();
            PollReceive = true;
        }

        // run in worker thread
        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            try
            {
                Receive(e.RawData);
                // wsh.OnReceiveData(e.RawData);
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