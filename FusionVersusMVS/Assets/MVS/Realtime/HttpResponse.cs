using System;
using System.Collections.Generic;

namespace MVS.Realtime
{
    public class MVMResponse
    {
        public List<MVMRes> ResponseMessage { get; set; }
        public DateTime Timestamp { get; set; }
        
    }

    public class MVMRes
    {
        public string Hostname { get; set; }
        public string RegionCode { get; set; }
        public string IpAddress { get; set; }

        public override string ToString()
        {
            return $"Hostname : {Hostname}, RegionCode : {RegionCode}, IpAddress : {IpAddress}";
        }
    }
    
    public class RoomListResponse
    {
        public List<RoomRes> ResponseMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RoomRes
    {
        public ulong RoomId { get; set; }
        public string Url { get; set; }
        public bool IsPending { get; set; }
        public string RoomName { get; set; }

        public override string ToString()
        {
            return $"RoomId : {RoomId}, Url : {Url}, IsPending : {IsPending}, RoomName : {RoomName}";
        }
    }

    public class RoomCreateResponse
    {
        public RoomResponseMessage ResponseMessage { get; set; }

        public override string ToString()
        {
            return $"Response : {ResponseMessage}";
        }
    }
    
    public class RoomJoinResponse
    {
        public RoomResponseMessage ResponseMessage { get; set; }
        
        public override string ToString()
        {
            return $"Response : {ResponseMessage}";
        }
    }

    public class RoomResponseMessage
    {
        public ulong UserId { get; set; }
        
        public ulong RoomId { get; set; }
        
        public string RoomName { get; set; }
        
        public string Token { get; set; }

        public string MvsUrl { get; set; }
        
        public bool CreationFlag { get; set; }

        public override string ToString()
        {
            return $"MvsUrl : {MvsUrl}, UserId : {UserId}, RoomId : {RoomId}, RoomName : {RoomName}, UserToken : {Token}, CreationFlag : {CreationFlag}";
        }
    }

}