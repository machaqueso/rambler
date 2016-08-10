using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace Rambler.Controllers
{
    public class YoutubeController : Controller
    {
        public IConfigurationRoot Configuration { get; }

        public YoutubeController()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets();

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Callback(string code)
        {
            var data = new List<KeyValuePair<string, string>>{
                new KeyValuePair<string,string>("code",code),
                new KeyValuePair<string,string>("client_id",Configuration["Authentication:Google:ClientId"]),
                new KeyValuePair<string,string>("client_secret",Configuration["Authentication:Google:ClientSecret"]),
                new KeyValuePair<string,string>("redirect_uri",Url.Action("Callback", "Youtube", null, Request.Scheme, null)),
                new KeyValuePair<string,string>("grant_type","authorization_code")
            };

            var response = await Post("https://accounts.google.com/o/oauth2/token", data);

            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(response);
            return View(googleToken);
        }

        public IActionResult Authorize()
        {
            var clientId = Configuration["Authentication:Google:ClientId"];
            var clientSecret = Configuration["Authentication:Google:ClientSecret"];

            var redirectUrl = WebUtility.UrlEncode(Url.Action("Callback", "Youtube", null, Request.Scheme, null));
            var oauthRequest = $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUrl}&scope=https://www.googleapis.com/auth/youtube&response_type=code&access_type=offline";

            return Redirect(oauthRequest);
        }

        private async Task<string> Get(string request)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with code: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> Post(string url, IList<KeyValuePair<string, string>> data)
        {
            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri("http://localhost:6740");
                var content = new FormUrlEncodedContent(data);

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    throw new HttpRequestException($"POST failed with code: {response.StatusCode}: {responseContent}");
                }

                return responseContent;
            }

        }

    }
}
