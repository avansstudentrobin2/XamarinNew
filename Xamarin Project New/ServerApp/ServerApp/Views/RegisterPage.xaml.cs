using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ServerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for clicking RegisterNewUser button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void RegisterNewUserButtonClicked(object sender, EventArgs args)
        {
            // Check password & check password has the same value
            if (Password.Text != CheckPassword.Text)
            {
                await DisplayAlert("Fout", "De ingevulde wachtwoorden zijn niet gelijk aan elkaar.", "OK");
                return;
            }

            // Nullcheck on fields
            if (Username.Text == null || Password.Text == null || CheckPassword.Text == null)
            {
                await DisplayAlert("Fout", "Niet alle velden zijn correct ingevuld.", "OK");
                return;
            }

            // Call function post user data
            var response = await PostUserAsync(Username.Text, Password.Text, CheckPassword.Text);

            // Check succesresponse
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Fout", "Er is iets niet goed gegaan. Probeer het opnieuw.", "OK");
                return;
            }

            // Function for setting the user in the db
            var responseUserId = await GetUserId(Username.Text);


            if (!responseUserId.IsSuccessStatusCode)
            {
                await DisplayAlert("Fout", "Er is iets niet goed gegaan. Probeer het opnieuw.", "OK");
                return;
            }

            // Function for trimming the userid
            var userid = TrimUserIdAsync(await responseUserId.Content.ReadAsStringAsync());
            
            // Function for setting the roles
            var responseRoles = await SetUserRoles(userid.Result.ToString());
            
            if(!responseRoles.IsSuccessStatusCode)
            {
                await DisplayAlert("Fout", "Er is iets niet goed gegaan. Probeer het opnieuw.", "OK");
                return;
            }

            // Return to MainPage
            var mainPage = new MainPage();
            await this.Navigation.PushModalAsync(mainPage);
            await DisplayAlert("Succes", "De gebruiker is met succes aangemaakt.", "OK");
        }

        /// <summary>
        /// Function to register an new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="checkpassword"></param>
        /// <returns>API ResponseMessage</returns>
        private async Task<HttpResponseMessage> PostUserAsync(string username, string password, string checkpassword)
        { 
            // Build an json
            string json = "{'Email': '" + username + "', 'Password': '" + password + "', 'ConfirmPassword': '" + checkpassword + "' }";
            // Post data to API, add to DB
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "api/Account/Register";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);

            return response;
        }

        /// <summary>
        /// Function for getting the userId from an username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>string userId</returns>
        private async Task<HttpResponseMessage> GetUserId(string username)
        {
            // Get userid from new user
            string getUseridUrl = Constants.Constants.ApiUrl + "api/Account/GetUserId/?username=" + username;
            var clientHttp = new HttpClient();
            var requestUserId = new HttpRequestMessage(HttpMethod.Get, getUseridUrl);
            var responseUserId = await clientHttp.SendAsync(requestUserId);

            return responseUserId;
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
        /// Function for setting the User roles to an new user
        /// </summary>
        /// <param name="userid"></param>
        /// <returns>Response Message from API</returns>
        private async Task<HttpResponseMessage> SetUserRoles(string userid)
        {
            string setRolesUrl = Constants.Constants.ApiUrl + "api/Account/Users/" + userid + "/roles";
            //Buid Json with rolename
            string jsonRoleData = "['Gebruiker']";
            // Post data to API, set user roles
            var getRoleClient = new HttpClient();
            var setRolesRequest = new HttpRequestMessage(HttpMethod.Post, setRolesUrl);
            setRolesRequest.Content = new StringContent(jsonRoleData, Encoding.UTF8, "application/json");
            var responseRoles = await getRoleClient.SendAsync(setRolesRequest);

            return responseRoles;
        }

        /// <summary>
        /// Event handler for clicking StopRegistering button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void StopRegisteringButtonClicked(object sender, EventArgs args)
        {
            // Go to mainpage
            var nextPage = new MainPage();
            await this.Navigation.PushModalAsync(nextPage);
        }
    }
}