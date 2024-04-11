
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MVS;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using WebSocketSharp;
using UInt64 = System.UInt64;

public partial class WebSocketHandler
{
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
    
    public Dictionary<PKT_ID, Func<byte[], int, bool>> handlerDic;
    private WebSocketConnection wsc;
    private SocketTcp _socketTcp;
    private IRealtimePeerListener Listener => _socketTcp.peerBase.Listener;
    public int clientNum;
    public bool isMain;

    private static int HeaderSize = 4; // Assuming 2 UInt16 for the header
    private const int MaxBufferSize = 1024 * 100; // 100KB, adjust as necessary

#region <Receive>

    private volatile PacketState currentState = PacketState.AwaitingHeader;
    private volatile int currentPacketDataSize;
    private Header currentHeader;

    private byte[] recvBuffer = new byte[MaxBufferSize];
    private int bufferStart = 0;
    private int bufferEnd = 0;

    private volatile Queue<Job> _jobQueue = new Queue<Job>();
    private volatile Stack<Job> _jobPool = new Stack<Job>();

#endregion

#region <Send>

    private static MemoryStream sendStream = new MemoryStream();
    private static BinaryWriter streamWriter = new BinaryWriter(sendStream);

#endregion

    public WebSocketHandler(SocketTcp socketTcp)
    {
        _socketTcp = socketTcp;
        clientNum = 1;
        isMain = clientNum == 1;
        HeaderSize = Marshal.SizeOf<Header>();

        // wsc = new WebSocketConnection();

        for (int i = 0; i < 20; i++)
        {
            _jobPool.Push(new Job(){header = new Header(), data = new byte[MaxBufferSize]});
        }
    }

    public WebSocketHandler(int clientNum)
    {
        this.clientNum = clientNum;
        isMain = (clientNum==1);
        
        HeaderSize = Marshal.SizeOf<Header>();

        wsc = new WebSocketConnection();

        for (int i = 0; i < 20; i++)
        {
            _jobPool.Push(new Job(){header = new Header(), data = new byte[MaxBufferSize]});
        }
    }

    public void ConnectServer(MVSRunner runner)
    {
        wsc.Start(this, runner);
    }

    public void Disconnect()
    {
        wsc.OnDestroy();
    }

    public void OnReceiveData(byte[] data)
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
    
    public void ProcessReceiveData()
    {
        // _socketTcp.peerBase.Listener.MVSDebug(DebugLevel.INFO, "ProcessReceiveData");
        var job = DequeueJob();
        while (job != null)
        {
            var id = job.header.id;
            var size = job.header.size;

            var result = handlerDic[(PKT_ID)id](job.data.SubArray(0, size - HeaderSize), size);
            PoolJob(job);

            job = DequeueJob();

            // if(GlobalCore.Instance.isViewMode) continue;

            // ButtonManager.SetResult((PKT_ID)id, result);
            if (result)
            {
                // if ((PKT_ID)id == PKT_ID.PKT_S_HEART_BEAT) continue;
                HighLightLog((PKT_ID)id);
            }
            else
            {
                Debug.LogError($"CID[{clientNum}] : {(PKT_ID)id} packet is inconsistent");
            }
        }
    }

    private void SendData(PKT_ID id, byte[] data)
    {
        _socketTcp.peerBase.Listener.MVSDebug(DebugLevel.INFO, $"SendData {id.ToString()}");
        Header header = new Header(){id = (UInt16)id, size = (UInt16)(data.Length+HeaderSize)};
        
        sendStream.SetLength(0);
        
        streamWriter.Write(header.id);
        streamWriter.Write(header.size);
        streamWriter.Write(data);

        // if(id != PKT_ID.PKT_C_HEART_BEAT)
        // {
        //     SendLog($"Send {id} size {header.size}");
        // }
        
        // wsc.Send(sendStream.ToArray());
        _socketTcp.SendPacket(sendStream.ToArray());
    }
    
    public bool ConsistencyCheck<T>(T left, T right, string name = "")
    {
        if (left.Equals(right))
        {
            return true;
        }
        // Debug.LogError($"{UIConsole.Yellow}{name}{UIConsole.ColorEnd} ConsistencyCheck Fail\n" +
        //                 $"-Expected value\t: {UIConsole.Sky}{left}{UIConsole.ColorEnd}\n" +
        //                 $"-Server value\t: {UIConsole.Sky}{right}{UIConsole.ColorEnd}\n"
        //                 );
        if (left is IConvertible && right is IConvertible)
        {
            try
            {
                // Debug.LogError(
                //     $"-Expected 16\t: {UIConsole.Sky}{left:X16}{UIConsole.ColorEnd}\n" +
                //     $"-Server 16\t\t: {UIConsole.Sky}{right:X16}{UIConsole.ColorEnd}\n"
                // );
            }
            catch
            {
                // ignored
            }
        }
        return false;
    }

    public void Log(object log)
    {
        // if(isMain) UIConsole.Log(log);
    }

    public void SendLog(object log)
    {
        // if(isMain) UIConsole.SendPacket(log);
    }

    public void HighLightLog(object log)
    {
        // if(isMain) UIConsole.Highlight(log);
    }

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

    public bool HandlePacket(PKT_ID id, ref byte[] data, ref int size)
    {
        try
        {
            if (handlerDic[id](data, size))
            {
                // if(id != PKT_ID.PKT_C_HEART_BEAT)
                //     HighLightLog(id);
                return true;
            }
            else
            {
                Debug.LogError($"{id} send fail");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{id} send fail");
            throw;
        }
    }
    
    public void SendPacket(PKT_ID pktID, byte[] data, int size)
    {
        HandlePacket(pktID, ref data, ref size);
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