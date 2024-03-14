using System.Collections.Generic;

namespace MVS.Realtime
{
    public class OperationResponse
    {
        public OperationCode OperationCode;

        public short ReturnCode;
        
        private ulong _sender;

        public byte[] FixedData;
        
        public List<byte[]> CustomData;

        public string ToString()
        {
            return $"OperationCode: {OperationCode}, ReturnCode: {ReturnCode}";
        }
    }
}