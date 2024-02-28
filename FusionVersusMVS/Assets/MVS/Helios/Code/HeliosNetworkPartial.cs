using MVS.Realtime;
using Protocol;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    public static partial class HeliosNetwork
    {
        public static void AddCallbackTarget(object target)
        {
            RealtimeClient.AddCallbackTarget(target);
        }
        
        public static void RemoveCallbackTarget(object target)
        {
            RealtimeClient.RemoveCallbackTarget(target);
        }

        private static IHeliosPrefabPool _prefabPool;

        public static IHeliosPrefabPool PrefabPool
        {
            get => _prefabPool;
            set
            {
                if (value == null)
                {
                    _prefabPool = new DefaultPrefabPool();
                }
                else
                {
                    _prefabPool = value;
                }
            }
        }

        private static void OnEvent(EventData eventData)
        {
            Debug.Log(eventData.code);
            switch (eventData.code)
            {
                case EventCode.PKT_S_ADD_NETWORK_OBJECTS:
                    var data = S_ADD_NETWORK_OBJECTS.Parser.ParseFrom(eventData.Data);
                    foreach (var objectInfo in data.ObjectInfos)
                    {
                        NetworkInstantiate(objectInfo);
                    }
                    break;
            }
        }

        private static void OnOperation(OperationResponse obj)
        {
            
        }

        private static void OnClientStateChanged(ClientState arg1, ClientState arg2)
        {
            RealtimeClient.RealtimePeer.Listener.MVSDebug(DebugLevel.INFO, arg2.ToString());
            switch (arg2)
            {
                case ClientState.ConnectedToMVS:
                    RealtimeClient.ConnectionCallbacksTarget.OnConnected();
                    break;
            }
        }

        public static void Service()
        {
            RealtimeClient.Service();
        }
    }
}