using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MVS.Realtime
{
    public class HttpModule
    {
        private static readonly HttpClient client = new HttpClient();

        // GET 요청 메소드
        public async Task<(string responseBody, HttpRequestException exception)> GetAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return (responseBody, null);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return (null, e);
            }
        }

        // POST 요청 메소드
        public async Task<(TResponse, HttpRequestException exception)> PostAsync<TRequest, TResponse>(string url, TRequest requestData)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(requestData);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                TResponse postResponse = JsonConvert.DeserializeObject<TResponse>(responseBody);
                return (postResponse, null);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return (default, e);
            }
        }
    }
}