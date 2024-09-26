using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MVS.Realtime
{
    public class HttpModule : MonoBehaviour
    {
        // GET 요청 메소드 (Coroutine)
        public static Coroutine GetAsync(MonoBehaviour owner, string url, Action<string, Exception> callback)
        {
            return owner.StartCoroutine(GetCoroutine(url, callback));
        }

        private static IEnumerator GetCoroutine(string url, Action<string, Exception> callback)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(webRequest.downloadHandler.text, null);
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    callback?.Invoke(null, new Exception(webRequest.error));
                }
            }
        }
        
        // Task 기반 비동기 GET 요청 메소드
        public static Task<string> GetAsync(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            GetAsync(ConnectionHandler.Instance, url, (result, exception) =>
            {
                if (exception != null)
                {
                    tcs.SetException(exception);
                }
                else
                {
                    tcs.SetResult(result);
                }
            });
            return tcs.Task;
        }

        // POST 요청 메소드 (Coroutine)
        public static Coroutine PostAsync<TRequest, TResponse>(MonoBehaviour owner, string url, TRequest requestData, Action<TResponse, Exception> callback)
        {
            return owner.StartCoroutine(PostCoroutine<TRequest, TResponse>(url, requestData, callback));
        }

        private static IEnumerator PostCoroutine<TRequest, TResponse>(string url, TRequest requestData, Action<TResponse, Exception> callback)
        {
            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseBody = webRequest.downloadHandler.text;
                    TResponse postResponse = JsonUtility.FromJson<TResponse>(responseBody);
                    callback?.Invoke(postResponse, null);
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    callback?.Invoke(default(TResponse), new Exception(webRequest.error));
                }
            }
        }
        
        // Task 기반 비동기 POST 요청 메소드
        public static Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestData)
        {
            var tcs = new TaskCompletionSource<TResponse>();
            PostAsync<TRequest, TResponse>(ConnectionHandler.Instance, url, requestData, (response, exception) =>
            {
                if (exception != null)
                {
                    tcs.SetException(exception);
                }
                else
                {
                    tcs.SetResult(response);
                }
            });
            return tcs.Task;
        }
    }
}
