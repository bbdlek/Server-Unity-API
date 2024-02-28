#nullable disable
namespace MVS.Realtime
{
    public class EventData
    {
        public EventCode code;

        private ulong _sender;
        
        public byte[] Data;

        public ulong Sender
        {
            get => _sender;
            set => _sender = value;
        }
    }
}