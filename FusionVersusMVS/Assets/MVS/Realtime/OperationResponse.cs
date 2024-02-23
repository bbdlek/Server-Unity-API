namespace MVS.Realtime
{
    public class OperationResponse
    {
        public OperationCode OperationCode;

        public short ReturnCode;

        public string ToString()
        {
            return $"OperationCode: {OperationCode}, ReturnCode: {ReturnCode}";
        }
    }
}