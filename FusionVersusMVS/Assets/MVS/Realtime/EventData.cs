#nullable disable
using System.Collections.Generic;
using Protocol;

namespace MVS.Realtime
{
    public class EventData
    {
        public int code;

        private ulong _sender;
        
        public byte[] FixedData;

        public CustomDic CustomData;

        public ulong Sender
        {
            get => _sender;
            set => _sender = value;
        }
    }
}