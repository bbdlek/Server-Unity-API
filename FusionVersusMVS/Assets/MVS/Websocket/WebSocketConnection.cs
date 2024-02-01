using System;
using WebSocketSharp;
using UnityEngine;
using Protocol;
using WebSocket = WebSocketSharp.WebSocket;

public class WebSocketConnection
{
    private WebSocket ws;
    private WebSocketHandler wsh;

    public void Start(WebSocketHandler wsh)
    {
        Debug.Log($"ws://{MVSAppSettings.Global.mvsURI}:{MVSAppSettings.Global.mvsPort}");
        this.wsh = wsh;
        ws = new WebSocket($"ws://{MVSAppSettings.Global.mvsURI}:{MVSAppSettings.Global.mvsPort}");

        ws.OnOpen += OnWebSocketOpen;
        ws.OnMessage += OnWebSocketMessage;
        ws.OnClose += OnWebSocketClose;

        // SSL/TLS 인증서 검증 무시 설정
        // ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12; // SSL/TLS 프로토콜 설정
        // ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        try
        {
            ws.ConnectAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError($"[CID:{wsh.clientNum}] Web Socket Secure Connection Fail");
            
            ws = new WebSocket($"ws://{MVSAppSettings.Global.mvsURI}:{MVSAppSettings.Global.mvsPort}\"");
            
            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnWebSocketMessage;
            ws.OnClose += OnWebSocketClose;

            try
            {
                ws.ConnectAsync();
            }
            catch (Exception ec)
            {
                Debug.LogError(ec);
                Debug.LogError($"[CID:{wsh.clientNum}] Web Socket Connection Fail");
                throw;
            }
        }
    }

    ~WebSocketConnection()
    {
        OnDestroy();
    }
    
    // run in worker thread
    private void OnWebSocketOpen(object sender, System.EventArgs e)
    {
        Debug.Log($"[CID:{wsh.clientNum}] WebSocket connected");
        // GlobalCore.IsConnected = true;
    }

    // run in worker thread
    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        // if (GlobalCore.Instance.printRowData)
        // {
        //     Debug.Log($"[CID:{wsh.clientNum}] OnReceive");
        //
        //     string rawData = "Raw Data : ";
        //     foreach (var b in e.RawData)
        //     {
        //         rawData += b.ToString("X2") + " ";
        //     }
        //     Debug.Log(rawData);
        // }
        
        try
        {
            wsh.OnReceiveData(e.RawData);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            throw;
        }
    }

    // run in worker thread
    private void OnWebSocketClose(object sender, CloseEventArgs e)
    {
        Debug.LogError("WebSocket closed with code: " + e.Code);
    }

    public void Send(byte[] data)
    {
        ws.Send(data);
    }

    public void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}
