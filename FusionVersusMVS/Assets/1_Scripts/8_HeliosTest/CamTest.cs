using System;
using System.Collections.Generic;
using Google.Protobuf;
using MVS.Helios;
using MVS.Helios.Utility;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;

public class CamTest : MonoBehaviorHeliosCallbacks
{
    public RenderTexture sendTexture;
    public RawImage readTexture;

    [HNSync] byte[] imageBytes;

    public override void Awake()
    {
        var texture2D = new Texture2D(192, 108, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, sendTexture.width, sendTexture.height), 0, 0);
        texture2D.Apply();

        imageBytes = texture2D.GetRawTextureData();
        
        base.Awake();
    }

    private void Start()
    {
        Debug.Log(typeof(byte[]).IsSerializable);
    }

    private void Update()
    {
        return;
        RenderTexture.active = sendTexture;
        var texture2D = new Texture2D(192, 108, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, sendTexture.width, sendTexture.height), 0, 0);
        texture2D.Apply();

        imageBytes = texture2D.GetRawTextureData();

        var data = new List<HeliosVariable>();
        ByteString byteString = ByteString.CopyFrom(imageBytes);
        data.Add(new HeliosVariable
        {
            NCustom = byteString
        });

        if(HeliosNetwork.InGroup)
            HeliosNetwork.RaiseEvent(1000, null, data);
    }


    private void SetTexture()
    {
        Texture2D receivedTexture = new Texture2D(sendTexture.width, sendTexture.height, TextureFormat.RGB24, false);
        receivedTexture.LoadRawTextureData(imageBytes);
        receivedTexture.Apply();

        readTexture.texture = receivedTexture;
    }
    
    private void SetTexture(ByteString data)
    {
        Texture2D receivedTexture = new Texture2D(sendTexture.width, sendTexture.height, TextureFormat.RGB24, false);
        receivedTexture.LoadRawTextureData(data.ToByteArray());
        receivedTexture.Apply();

        readTexture.texture = receivedTexture;
    }

    public override void OnConnectedToMasterServer()
    {
        
    }

    public override void OnConnected()
    {
        
    }

    public override void OnDisconnected()
    {
        
    }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }

    public override void OnCreatedRoom()
    {
        
    }

    public override void OnCreatedRoomFailed(string message)
    {
        
    }

    public override void OnJoinedRoom()
    {
        
    }

    public override void OnJoinedRoomFailed(string message)
    {
        
    }

    public override void OnLeftRoom()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
    }

    public override void OnPlayerLeftGroup(Player otherPlayer)
    {
        
    }

    public override void OnEvent(EventData eventData)
    {
        if (eventData.code == 1000)
        {
            Debug.Log(eventData.CustomData.Count);
            // SetTexture(eventData.CustomData[0].NCustom);
        }
    }

    public override void OnErrorInfo(string errorInfo)
    {
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        
    }

    public override void OnObjectInstantiated(ObjectInfo objectInfo)
    {
        
    }

    public override void OnObjectDestroyed(ObjectInfo objectInfo)
    {
        
    }

    public override void OnCreatedGroup()
    {
        
    }

    public override void OnCreatedGroupFailed(string message)
    {
        
    }

    public override void OnJoinedGroup()
    {
        
    }

    public override void OnJoinedGroupFailed(string message)
    {
        
    }

    public override void OnLeftGroup()
    {
        
    }

    public override void OnPlayerEnteredGroup(Player newPlayer)
    {
        
    }
}