using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace MVS.Realtime
{
    [Serializable]
    public class MVMResponse
    {
        public List<MVMRes> responseMessage;
        public DateTime timestamp;

    }

    [Serializable]
    public class MVMRes
    {
        public string hostname;
        public string regionCode;
        public string ipAddress;

        public override string ToString()
        {
            return $"Hostname : {hostname}, RegionCode : {regionCode}, IpAddress : {ipAddress}";
        }
    }
    
    [Serializable]
    public class RoomListResponse
    {
        public List<RoomRes> responseMessage;
        public DateTime timestamp;
    }

    [Serializable]
    public class RoomRes
    {
        public ulong roomId;
        public string url;
        public bool isPending;
        public string name;

        public override string ToString()
        {
            return $"RoomId : {roomId}, Url : {url}, IsPending : {isPending}, RoomName : {name}";
        }
    }

    [Serializable]
    public class RoomCreateResponse
    {
        public RoomResponseMessage responseMessage;
        public string timestamp;

        public override string ToString()
        {
            return $"Response : {responseMessage}, Timestamp: {timestamp}";
        }
    }
    
    [Serializable]
    public class RoomJoinResponse
    {
        public RoomResponseMessage responseMessage;
        public string timestamp;
        
        public override string ToString()
        {
            return $"Response : {responseMessage}, Timestamp: {timestamp}";
        }
    }

    [Serializable]
    public class RoomResponseMessage
    {
        public ulong userId;
        public string token;
        public string mvsUrl;
        public string roomName;
        public ulong roomId;
        public bool creationFlag;

        public override string ToString()
        {
            return $"MvsUrl : {mvsUrl}, UserId : {userId}, RoomId : {roomId}, RoomName : {roomName}, UserToken : {token}, CreationFlag : {creationFlag}";
        }
    }

}