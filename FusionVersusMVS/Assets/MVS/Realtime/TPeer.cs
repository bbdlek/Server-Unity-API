using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Protocol;
using WebSocketSharp;

namespace MVS.Realtime
{
    #nullable disable
    public class TPeer : PeerBase
    {
        public Dictionary<PKT_ID, Func<byte[], int, bool>> handlerDic;
        
        public enum PKT_ID
        {
            PKT_C_OPERATION = 1000,
            PKT_S_OPERATION = 1001,
            PKT_S_EVENT = 1002,
        };
        
        public struct Header
        {
            public UInt16 id;
            public UInt16 size;
        }
        
        private enum PacketState
        {
            AwaitingHeader,
            AwaitingData,
            Complete
        }

        private class Job
        {
            public Header header;
            public byte[] data;
        }
        
        private static int HeaderSize = 4; // Assuming 2 UInt16 for the header
        private const int MaxBufferSize = 1024 * 100; // 100KB, adjust as necessary
        
        // private MVSWebSocket _mvsWebSocket;
        protected internal bool DoFraming = true;
        
        internal override bool Connect(string serverAddress, string appId, ServerConnection serverType)
        {
            if (!realtimeSocket.Connect())
                return false;
            HeaderSize = Marshal.SizeOf<Header>();
            for (int i = 0; i < 20; i++)
            {
                _jobPool.Push(new Job(){header = new Header(), data = new byte[MaxBufferSize]});
            }
            Listener.MVSDebug(DebugLevel.INFO, "TPeer Connect()");
            peerConnectionState = ConnectionStateValue.Connecting;
            return true;
        }

        public override void OnConnect()
        {
            Listener.MVSDebug(DebugLevel.INFO, "TPeer OnConnect()");
            handlerDic = new Dictionary<PKT_ID, Func<byte[], int, bool>>();
            handlerDic[PKT_ID.PKT_S_OPERATION] = (bytes, len) => PacketHandler<S_OPERATION>.Handling(Handle_S_OPERATION, bytes, len);
            handlerDic[PKT_ID.PKT_S_EVENT] = (bytes, len) => PacketHandler<S_EVENT>.Handling(Handle_S_EVENT, bytes, len);
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
        
        private static MemoryStream sendStream = new MemoryStream();
        private static BinaryWriter streamWriter = new BinaryWriter(sendStream);

        internal override bool SendPacket(byte[] data, int size)
        {
            //TCP Data 생성
            Header header = new Header()
                { id = (UInt16)PKT_ID.PKT_C_OPERATION, size = (UInt16)(data.Length + HeaderSize) };
            
            sendStream.SetLength(0);
        
            streamWriter.Write(header.id);
            streamWriter.Write(header.size);
            streamWriter.Write(data);
            
            realtimeSocket.Send(sendStream.ToArray());
            return true;
        }
        
        private volatile PacketState currentState = PacketState.AwaitingHeader;
        private volatile int currentPacketDataSize;
        private Header currentHeader;

        private byte[] recvBuffer = new byte[MaxBufferSize];
        private int bufferStart = 0;
        private int bufferEnd = 0;

        private volatile Queue<Job> _jobQueue = new Queue<Job>();
        private volatile Stack<Job> _jobPool = new Stack<Job>();

        internal override bool ProcessIncomingData()
        {
            var job = DequeueJob();
            while (job != null)
            {
                var id = job.header.id;
                var size = job.header.size;

                var result = handlerDic[(PKT_ID)id](job.data.SubArray(0, size - HeaderSize), size);
                PoolJob(job);

                job = DequeueJob();
                
                if (result)
                {
                }
                else
                {
                    Listener.MVSDebug(DebugLevel.ERROR, $"{(PKT_ID)id} packet is inconsistent");
                }
            }
            return true;
        }

        internal override bool ProcessOutgoingData()
        {
            return true;
        }

        internal override void ReceiveIncomingData(byte[] data)
        {
            EnsureCapacity(data.Length);
            Array.Copy(data, 0, recvBuffer, bufferEnd, data.Length);
            bufferEnd += data.Length;

            while (true)
            {
                switch (currentState)
                {
                    case PacketState.AwaitingHeader:
                        if (bufferEnd - bufferStart >= HeaderSize)
                        {
                            currentHeader.id = BitConverter.ToUInt16(recvBuffer, bufferStart);
                            currentHeader.size = BitConverter.ToUInt16(recvBuffer, bufferStart + 2);
                            currentPacketDataSize = currentHeader.size - HeaderSize;
                            currentState = PacketState.AwaitingData;
                            bufferStart += HeaderSize;
                        }
                        else
                        {
                            return;
                        }
                        break;

                    case PacketState.AwaitingData:
                        if (bufferEnd - bufferStart >= currentPacketDataSize)
                        {
                            var job = PopJob();
                            job.header = currentHeader;
                            Array.Copy(recvBuffer, bufferStart, job.data, 0, currentPacketDataSize);
                            EnqueueJob(job);
                            bufferStart += currentPacketDataSize;
                            currentState = PacketState.AwaitingHeader;
                        }
                        else
                        {
                            return;
                        }
                        break;
                }
            }
        }
        
        private void EnsureCapacity(int incomingDataSize)
        {
            if (bufferEnd + incomingDataSize > recvBuffer.Length)
            {
                if (bufferStart > 0)
                {
                    Array.Copy(recvBuffer, bufferStart, recvBuffer, 0, bufferEnd - bufferStart);
                    bufferEnd -= bufferStart;
                    bufferStart = 0;
                }
            }
        }

        #region RECV_Functions
        bool Handle_S_OPERATION(byte[] data)
        {
            var packet = S_OPERATION.Parser.ParseFrom(data);
            if (packet.Result != Result.Success)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"packet result : {packet.Result}");
                return false;
            }

            OperationResponse opData = new OperationResponse
            {
                OperationCode = (OperationCode)packet.OperationCode,
                ReturnCode = 0,
                FixedData = packet.FixedData.ToByteArray()
            };
            opData.CustomData = packet.CustomData;
            Listener.OnOperationResponse(opData);

            return true;
        }

        bool Handle_S_EVENT(byte[] data)
        {
            var packet = S_EVENT.Parser.ParseFrom(data);
            if (packet.Result != Result.Success)
            {
                Listener.MVSDebug(DebugLevel.ERROR, $"packet result : {packet.Result}");
                return false;
            }
            EventData eventData = new EventData
            {
                code = packet.EventCode,
                Sender = packet.Sender.PlayerID,
                FixedData = packet.FixedData.ToByteArray()
            };
            eventData.CustomData = packet.CustomData;
            Listener.OnEvent(eventData);

            return true;
        }

        #endregion RECV_Functions
        
        private void PoolJob(Job job)
        {
            lock (_jobPool)
            {
                _jobPool.Push(job);
            }
        }

        private UInt64 count;
        private Job PopJob()
        {
            if (_jobPool.Count > 0)
            {
                lock (_jobPool)
                {
                    return _jobPool.Pop();
                }
            }
            else
            {
                return new Job(){header = new Header(), data = new byte[MaxBufferSize]};
            }
        }

        private void EnqueueJob(Job job)
        {
            lock (_jobQueue)
            {
                _jobQueue.Enqueue(job);
            }
        }

        private Job DequeueJob()
        {
            lock (_jobQueue)
            {
                if (_jobQueue.Count > 0) return _jobQueue.Dequeue();
                else return null;
            }
        }
    }
    static class PacketHandler<PacketType>
    {
        public static bool Handling(Func<byte[], bool> func, byte[] data, int len)
        {
            var res = func(data);
            return res;
        }
    }
    
}