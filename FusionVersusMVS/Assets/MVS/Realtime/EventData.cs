#nullable disable
using System.Collections.Generic;

namespace MVS.Realtime
{
    public class EventData
    {
        public int code;

        private ulong _sender;
        
        public byte[] FixedData;

        public List<byte[]> CustomData;

        public ulong Sender
        {
            get => _sender;
            set => _sender = value;
        }
    }
}