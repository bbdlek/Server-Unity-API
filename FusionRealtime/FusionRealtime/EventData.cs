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

        public List<Protocol.HeliosVariable> CustomData = new List<Protocol.HeliosVariable>();

        public ulong Sender
        {
            get => _sender;
            set => _sender = value;
        }
    }
}