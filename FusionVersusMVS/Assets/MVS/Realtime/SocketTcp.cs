using System;
using System.Net.Sockets;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace MVS.Realtime
{
    public class SocketTcp : RealtimeSocketConnection, IDisposable
    {
        private Socket _socket;
        private TcpClient _client;
        
        private readonly object syncer = new object();
        
        [Preserve]
        public SocketTcp(PeerBase peerBase) : base(peerBase)
        {
            Listener.MVSDebug(DebugLevel.INFO, "SocketTcp, .Net, Unity");
            
            // 데이터 수신을 폴링하지 않음
            PollReceive = false;
        }

        ~SocketTcp() => Dispose();

        public override bool Connect()
        {
            lock (syncer)
            {
                if (!base.Connect())
                    return false;
                State = RealtimeSocketState.Connecting;
            }
            new Thread(DnsAndConnect)
            {
                IsBackground = true
            }.Start();
            return true;
        }

        internal void DnsAndConnect()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //TODO : TimeOut Settings
                _socket.NoDelay = true;
                _socket.ReceiveTimeout = 5000;
                _socket.SendTimeout = 5000;
                // _socket.ReceiveTimeout = peerBase.DisconnectTimeout;
                // _socket.SendTimeout = peerBase.DisconnectTimeout;

                _socket.Connect(ServerAddress, int.Parse(ServerPort));
                Listener.MVSDebug(DebugLevel.INFO, "Connected To Server With TCP");
            }
            catch (SecurityException e)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"SecurityException : {e}");
            }
            catch (SocketException e)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"SocketException : {e}, ErrorCode : {e.ErrorCode}");
            }
            catch (Exception e)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"Exception : {e}");
            }

            if (_socket == null || !_socket.Connected)
            {
                Listener.MVSDebug(DebugLevel.ERROR, "Failed To Connect to Server.");
            }
            else
            {
                State = RealtimeSocketState.Connected;
                peerBase.OnConnect();
                // new Thread(ReceiveLoop)
                // {
                //     IsBackground = true
                // }.Start();
                Task.Run(ReceiveLoopAsync);
            }
        }
        
        internal async Task ReceiveLoopAsync()
        {
            try
            {
                while (State == RealtimeSocketState.Connected && _socket != null && _socket.Connected)
                {
                    byte[] buffer = new byte[1024]; // 데이터를 읽을 버퍼를 준비합니다.
                    int bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None); // 비동기로 데이터를 읽습니다.

                    if (bytesRead > 0)
                    {
                        // 받은 데이터를 처리합니다.
                        byte[] dataToProcess = ExtractNeededData(buffer, bytesRead);
                        if (dataToProcess.Length > 0)
                        {
                            Listener.MVSDebug(DebugLevel.INFO, BitConverter.ToString(dataToProcess));
                            peerBase.ReceiveIncomingData(dataToProcess);
                        }
                    }
                    else
                    {
                        // 소켓 연결이 끊어졌을 경우에 대한 처리를 여기에 추가합니다.
                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // 소켓이 이미 해제되었을 때 발생하는 예외입니다.
                Listener.MVSDebug(DebugLevel.INFO, "Socket already disposed.");
            }
            catch (Exception e)
            {
                if (State != RealtimeSocketState.Disconnecting && State != 0)
                {
                    Listener.MVSDebug(DebugLevel.INFO, "Exception in Disconnect(): " + e);
                }
            }
            finally
            {
                lock (syncer)
                {
                    if (State == RealtimeSocketState.Disconnecting || State == 0)
                    {
                        // Optionally perform cleanup here
                    }
                    else
                    {
                        Disconnect();
                    }
                }
            }
        }

        internal void ReceiveLoop()
        {
            try
            {
                while (State == RealtimeSocketState.Connected)
                {
                    byte[] buffer = new byte[1024]; // 데이터를 읽을 버퍼를 준비합니다.
                    int bytesRead = _socket.Receive(buffer); // 데이터를 읽습니다.
        
                    if (bytesRead > 0)
                    {
                        // 받은 데이터를 처리합니다. 이 예시에서는 필요한 데이터만을 추출하여 ReceiveIncomingData 메서드로 전달합니다.
                        byte[] dataToProcess = ExtractNeededData(buffer, bytesRead);
                        if (dataToProcess.Length > 0)
                        {
                            peerBase.ReceiveIncomingData(dataToProcess);
                        }
                    }
                    else
                    {
                        // 소켓 연결이 끊어졌을 경우에 대한 처리를 여기에 추가합니다.
                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // 소켓이 이미 해제되었을 때 발생하는 예외입니다.
                Listener.MVSDebug(DebugLevel.INFO, "Socket already disposed.");
            }
            catch (Exception e)
            {
                if (State != RealtimeSocketState.Disconnecting && State != 0)
                {
                    Listener.MVSDebug(DebugLevel.INFO, "Exception in Disconnect(): " + e);
                }
            }
            lock (syncer)
            {
                if (State == RealtimeSocketState.Disconnecting || State == 0)
                    return;
                Disconnect();
            }
        }
        
        private byte[] ExtractNeededData(byte[] buffer, int bytesRead)
        {
            // 여기에 필요한 데이터를 추출하는 로직을 작성합니다.
            // 예를 들어, 특정 패턴이나 프로토콜 규약에 따라 데이터를 처리하여 필요한 부분만을 추출할 수 있습니다.

            // 이 예시에서는 그냥 모든 데이터를 반환합니다.
            byte[] dataToProcess = new byte[bytesRead];
            Array.Copy(buffer, 0, dataToProcess, 0, bytesRead);
            return dataToProcess;
        }

        public override bool Disconnect()
        {
            Listener.MVSDebug(DebugLevel.INFO, "SocketTcp Disconnect()");
            lock (syncer)
            {
                State = RealtimeSocketState.Disconnecting;
                if (_socket != null)
                {
                    try
                    {
                        _socket.Close();
                    }
                    catch (Exception e)
                    {
                        Listener.MVSDebug(DebugLevel.INFO, "Exception in Disconnect(): " + e);
                            
                    }
                }
                State = RealtimeSocketState.Disconnected;
            }
            return true;
        }

        public override bool Send(byte[] data)
        {
            if(_socket != null && State == RealtimeSocketState.Connected)
                _socket.Send(data, 0, data.Length, SocketFlags.None);
            return true;
        }

        public override bool Receive(byte[] data)
        {
            return true;
        }

        public void Dispose()
        {
            State = RealtimeSocketState.Disconnecting;
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                        _socket.Close();
                }
                catch (Exception e)
                {
                    Listener.MVSDebug(DebugLevel.INFO, "Exception in Dispose(): " + e);
                }
            }
            _socket = null;
            State = RealtimeSocketState.Disconnected;
        }
    }
}