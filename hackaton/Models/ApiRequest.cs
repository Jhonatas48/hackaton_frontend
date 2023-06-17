using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;

namespace hackaton.Models
{
    public class ApiRequest
    {


        public static async Task<List<User>> getUsers()
        {


            HttpResponseMessage response = await SendHttpRequest("https://localhost:794/Users/getusers", HttpMethod.Get, "1", null);
            return null;
        }
        private static async Task<HttpResponseMessage> SendHttpRequest(string url, HttpMethod method, string token, object data = null)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpRequestMessage request = new HttpRequestMessage(method, url);

                if (data != null)
                {
                    // Serializar o objeto em formato JSON
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }
    }
}