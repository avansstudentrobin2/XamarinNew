using Newtonsoft.Json;
using ServerApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ServerApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewerr
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            CheckLoginAsync();
        }

        /// <summary>
        /// This function checks if the user is logged in
        /// </summary>
        /// <returns>Await new Page</returns>
        private async Task CheckLoginAsync()
        {
            // Check user is allready
            if(Application.Current.Properties["bearer"] != null && Application.Current.Properties["userid"] != null)
            {
                // Check servers of current user
                var nextPage = await CheckServersAsync(Application.Current.Properties["userid"].ToString());

                // If the user has servers, go to page where user can select page
                // Else enter a new server page
                if (nextPage.ToString() == "true")
                {
                    var page = new ServerPage();
                    await this.Navigation.PushModalAsync(page);
                }
                else
                {
                    var page = new EnterServerPage();
                    await this.Navigation.PushModalAsync(page);
                }
            }
        }

        /// <summary>
        /// Event handler on loginbutton clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void OnLoginButtonClicked(object sender, EventArgs args)
        {
            // Set all input fields to username & password
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("Username", Username.Text));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "password"));
            nvc.Add(new KeyValuePair<string, string>("Password", Password.Text));

            try
            {
                // Get user bearer token
                JsonValue itemsJson = await GetBearerTokenAsync(nvc);
                // Set bearer value
                string bearer = itemsJson["access_token"];
                // Get userid from user
                var userResponse = await GetUserId(itemsJson["userName"]);
                // Trim userresponse, get id
                var userid = TrimUserIdAsync(await userResponse.Content.ReadAsStringAsync());

                // Set bearer token in Storage
                Application.Current.Properties["bearer"] = bearer;
                // Set userid is Storage
                Application.Current.Properties["userid"] = userid.Result.ToString();

                // Check user servers in DB
                var nextPage = await CheckServersAsync(userid.Result.ToString());

                // If the user has servers, go to page where user can select page
                // Else enter a new server page
                if (nextPage.ToString() == "true")
                {
                    var page = new ServerPage();
                    await this.Navigation.PushModalAsync(page);
                }
                else
                {
                    var page = new EnterServerPage();
                    await this.Navigation.PushModalAsync(page);
                }
            } catch (Exception ex)
            {
                // Something is wrong with the API Connection
                await DisplayAlert("Fout", "Er is iets foutgegaan. Check je internetverbinding en probeer het opnieuw.", "OK");
                return;
            }
        }

        /// <summary>
        /// Function to get a user logged in
        /// </summary>
        /// <param name="list">List with userdata</param>
        /// <returns>Response from ASP Identity. Username, Bearer token, etc</returns>
        public async Task<JsonValue> GetBearerTokenAsync(List<KeyValuePair<string, string>> list)
        {
            // Post credentials to API, get bearer token
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "Token";
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(list) };
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // The credentials are wrong
                await DisplayAlert("Fout", "De ingevoerde gegevens zijn niet juist. Probeer het opnieuw.", "OK");
                return null;
            }
            // Parse to Json, get response
            return JsonValue.Parse(JsonConvert.DeserializeObject(responseString).ToString());
        }

        /// <summary>
        /// Checks if user has servers on his name
        /// </summary>
        /// <param name="userid"></param>
        /// <returns>True of false</returns>
        public async Task<string> CheckServersAsync(string userid)
        {
            // Api call check servers
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "api/Server/CheckServers/" + userid;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + Application.Current.Properties["bearer"].ToString());
            var response = await client.SendAsync(request);
            // Return response from API
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString.ToString();
        }

        /// <summary>
        /// Function for trimming an API response to readable userid
        /// </summary>
        /// <param name="userid"></param>
        /// <returns>string userId</returns>
        private async Task<string> TrimUserIdAsync(string userid)
        {
            // Trim userid to readable
            char[] charsToTrim = { '"', '\'' };
            userid = userid.Trim(charsToTrim);

            return userid;
        }

        /// <summary>
        /// Function for getting the userId from an username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>HttpResponseMessage with userId</returns>
        public async Task<HttpResponseMessage> GetUserId(string username)
        {
            // Get userid from new user
            string getUseridUrl = Constants.Constants.ApiUrl + "api/Account/GetUserId/?username=" + username;
            var clientHttp = new HttpClient();
            var requestUserId = new HttpRequestMessage(HttpMethod.Get, getUseridUrl);
            var responseUserId = await clientHttp.SendAsync(requestUserId);

            return responseUserId;
        }
        /// <summary>
        /// Event handler RegisterNewUser-button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void RegisterNewUserButtonClicked(object sender, EventArgs args)
        {
            // Go to new RegisterPage
            var nextPage = new RegisterPage();
            await this.Navigation.PushModalAsync(nextPage);
        }
    }
}
