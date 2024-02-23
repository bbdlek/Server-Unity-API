namespace MVS.Realtime
{
    public interface IRealtimePeerListener
    {
        void MVSDebug(DebugLevel debugLevel, string msg);

        void OnOperationResponse(OperationResponse operationResponse);

        void OnStatusChanged(StatusCode statusCode);

        void OnEvent(EventCode eventCode);
    }
}
