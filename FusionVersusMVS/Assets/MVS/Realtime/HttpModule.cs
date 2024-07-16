using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MVS.Realtime
{
    public class HttpModule
    {
        // GET 요청 메소드
        public async Task<(string responseBody, Exception exception)> GetAsync(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return (webRequest.downloadHandler.text, null);
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    return (null, new Exception(webRequest.error));
                }
            }
        }

        // POST 요청 메소드
        public async Task<(TResponse response, Exception exception)> PostAsync<TRequest, TResponse>(string url, TRequest requestData)
        {
            string jsonData = JsonConvert.SerializeObject(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    TResponse postResponse = JsonConvert.DeserializeObject<TResponse>(responseBody);
                    return (postResponse, null);
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    return (default(TResponse), new Exception(webRequest.error));
                }
            }
        }
    }
}
