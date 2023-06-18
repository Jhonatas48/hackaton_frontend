using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;

namespace hackaton.Models
{
    public class ApiRequest
    {
        private static string _url;
        private static string _token;
        public ApiRequest() {
            // Criar uma instância do ConfigurationBuilder
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            // Construir a configuração
            var configuration = configBuilder.Build();
            _token = configuration["Autentication:Token"];
            _url = configuration["Autentication:Backend_Url"];
        } 

        public static async Task<User> createUser(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Home/Register", HttpMethod.Post, user);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }

        public static async Task<User> modifyUserLogged(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Client/Update", HttpMethod.Post, user);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }

        public static async Task<User> deleteUser(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Client/Delete", HttpMethod.Delete, user);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }
        public static async Task<List<User>> getUsers()
        {
           
            HttpResponseMessage response = await SendHttpRequest("/Users/getusers", HttpMethod.Get, null);

             if(!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(jsonResponse);
        }


        private static async Task<HttpResponseMessage> SendHttpRequest(string url, HttpMethod method, object data = null)
        {
            // Criar uma instância do ConfigurationBuilder
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            // Construir a configuração
            var configuration = configBuilder.Build();
            _token = configuration["Autentication:Token"];
            _url = configuration["Autentication:Backend_Url"];
            url= _url+ url;
            using (HttpClient client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

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