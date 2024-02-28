namespace MVS.Realtime
{
    public class OperationResponse
    {
        public OperationCode OperationCode;

        public short ReturnCode;

        public byte[] Data;

        public string ToString()
        {
            return $"OperationCode: {OperationCode}, ReturnCode: {ReturnCode}";
        }
    }
}