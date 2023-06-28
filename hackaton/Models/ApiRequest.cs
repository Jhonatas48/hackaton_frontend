using frontend_hackaton.Models;
using frontend_hackaton.Models.Desserializers;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Azure.Core;
using Azure;
using Microsoft.AspNet.SignalR;

namespace hackaton.Models
{
    public class ApiRequest
    {
        private static string _url;
        private static string _token;


        public static async Task<bool> requestLogin(User user) {

            HttpResponseMessage response = await SendHttpRequest("/Home/Login", HttpMethod.Post, user);
            string json = await response.Content.ReadAsStringAsync();
            
            return response.IsSuccessStatusCode;
        
        }
       public static async Task<ApiResponse<User>> createUser(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Home/Register", HttpMethod.Post, user);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {


                var errorResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<User>>(json);

                return errorResponse;
                
            }

            user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);

            return new ApiResponse<User> { Sucess = true, classObject = user };
        }
        public static async Task<User> getUserToModify(int? userId,User userAdmin)
        {
            if(userId == null || userId==0)
            {
                return null;
            }

            HttpResponseMessage response = await SendHttpRequest("/Users/Edit/" + userId, HttpMethod.Get, userAdmin);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }

        public static async Task<ApiResponse<User>> modifyUserLogged(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Client/Edit", HttpMethod.Post, user);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<User>>(jsonResponse);
                apiResponse.statusCode = (int)response.StatusCode;
                return apiResponse;
            }
          
            user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);

            return new ApiResponse<User> { Sucess = true, classObject = user };
        }

        public static async Task<User> deleteUserLogged(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Client/Delete", HttpMethod.Post, user);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }


        //QrCode

        public static async Task<byte[]> CreateQrCode(User user) {

            HttpResponseMessage response = await SendHttpRequest("/QrCode/GenerateQrCode", HttpMethod.Post, user);

            if (response.IsSuccessStatusCode)
            {
                // Lê o conteúdo da resposta como bytes
                byte[] qrCodeBytes = await response.Content.ReadAsByteArrayAsync();
                return qrCodeBytes;
            }
            return null;
            
        }

        public static async Task<ApiResponse<QrCode>> validateQrCode(QrCode qrCode) {

            return null;
        }
        
        //Admin
        public static async Task<ApiResponse<User>> modifyUser(User user)
        {
            HttpResponseMessage response = await SendHttpRequest("/Users/Edit/"+user.Id, HttpMethod.Post, user);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {

                var errorResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<User>>(json);

                return errorResponse;

            }

            user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);

            return new ApiResponse<User> { Sucess = true, classObject = user };
        }

        public static async Task<User> deleteUser(int userId,User userAdmin)
        {
            HttpResponseMessage response = await SendHttpRequest("/Users/Delete/"+userId, HttpMethod.Post, userAdmin);

            if (!response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(jsonResponse);
        }

        public static async Task<List<User>> getUsers() {
            return await getUsers(null);
        }
        public static async Task<List<User>> getUsers(string search)
        {
            string request = "/Users";
          
            if(search == null || search.IsNullOrEmpty()) {
                request = "/Users" + search;
            }
            else
            {
                request = "/Users/search?searchQuery=" + search;
            }
            HttpResponseMessage response = await SendHttpRequest(request, HttpMethod.Get, null);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(jsonResponse);
        }

        //Schedules
        public static async Task<ApiResponse<Schedule>> createSchedule(Schedule schedule)
        {
            /*
            HttpResponseMessage response = await SendHttpRequest("/Schedule/Create", HttpMethod.Post, schedule);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("sucesso");
                return null;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            if(jsonResponse == null || jsonResponse.IsNullOrEmpty()) {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Schedule>(jsonResponse);*/

            HttpResponseMessage response = await SendHttpRequest("/Schedule/Create", HttpMethod.Post, schedule);
            string json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {

                var errorResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<Schedule>>(json);

                 return errorResponse;

               }

                schedule = Newtonsoft.Json.JsonConvert.DeserializeObject<Schedule>(json);

                return new ApiResponse<Schedule> { Sucess=true,classObject=schedule};
         }

        public static async Task<List<Schedule>> getSchedules()
        {
            return await getSchedules(0);
        }
        public static async Task<List<Schedule>> getSchedules(int userid)
        {
            HttpResponseMessage response;


            if (userid == 0)
            {
                response = await SendHttpRequest("/Schedule/Index", HttpMethod.Get, null);
            }
            else
            {
                response = await SendHttpRequest("/Schedule/?userid=" + userid, HttpMethod.Get, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(jsonResponse);
        }

        public static async Task<Schedule> deleteSchedule(int id)
        {
            HttpResponseMessage response;


            if (id == 0)
            {
                return null;
            }
            else
            {
                response = await SendHttpRequest("/Schedule/Delete/" + id, HttpMethod.Post, null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string json = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Schedule>(json);
           
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
            Console.WriteLine("URL: "+url);
            using (HttpClient client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpRequestMessage request = new HttpRequestMessage(method, url);

                if (data != null)
                {
                    // Serializar o objeto em formato JSON
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                    Console.WriteLine(jsonData);
                    request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage response = await client.SendAsync(request);

                return response;
            }
        }

    }
}