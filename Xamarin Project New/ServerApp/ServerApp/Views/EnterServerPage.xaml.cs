using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ServerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterServerPage : ContentPage
    {
        public EnterServerPage()
        {
            // If User is not logged in, go back to login page
            if (string.IsNullOrEmpty(Application.Current.Properties["userid"].ToString()) || string.IsNullOrEmpty(Application.Current.Properties["bearer"].ToString()))
            {
                LogOutFromSystemAsync();
            }
            InitializeComponent();
            // Enable / disable go to servers button
            CheckHasServersAsync();
        }

        /// <summary>
        /// Function for checking the servers from an user. Set CancelGoToServersButton visible/invisible.
        /// </summary>
        /// <returns>Void function</returns>
        private async Task CheckHasServersAsync()
        {
            // Api call check servers
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "api/Server/CheckServers/" + Application.Current.Properties["userid"];
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "bearer " + Application.Current.Properties["bearer"].ToString());
            var response = await client.SendAsync(request);
            // Return response from API
            var responseString = await response.Content.ReadAsStringAsync();
            // If user has a server, show button go back to servers.
            if (responseString.ToString() == "true")
            {
                CancelGoToServersButton.IsVisible = true;
            }
            else
            {
                CancelGoToServersButton.IsVisible = false;
            }
        }

        /// <summary>
        /// Event handler for clicking RegisterNewServer-button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void RegisterNewServerButtonClicked(object sender, EventArgs args)
        {
            // Check on empty fields
            if (IPadres.Text == null || ServerNaam.Text == null || Wachtwoord.Text == null)
            {
                // If user has entered default server, do nothing
                if (ServerNaam.Text.ToLower() != "default")
                {
                    await DisplayAlert("Fout", "Velden zijn niet goed ingevuld. Probeer het opnieuw.", "OK");
                    return;
                }
            }

            // Set all input fields to IPaddress & UserId
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("ServerName", ServerNaam.Text));
            nvc.Add(new KeyValuePair<string, string>("UserId", Application.Current.Properties["userid"].ToString()));
            nvc.Add(new KeyValuePair<string, string>("ServerIp", IPadres.Text));
            nvc.Add(new KeyValuePair<string, string>("Password", Wachtwoord.Text));

            try
            {
                // Post credentials to API, get id from Server
                var client = new HttpClient();
                string url = Constants.Constants.ApiUrl + "api/Server/AddServer";
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };
                request.Headers.Add("Authorization", "Bearer " + Application.Current.Properties["bearer"].ToString());
                var response = await client.SendAsync(request);

                // If the response has success
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Succes", "De server is met succes toegevoegd.", "OK");
                    var nextPage = new ServerPage();
                    await this.Navigation.PushModalAsync(nextPage);
                } 
                else
                {
                    await DisplayAlert("Fout", "Er is iets niet goed gegaan. Probeer het opnieuw.", "OK");
                    return;
                }
            }
            catch
            {
                await DisplayAlert("Fout", "Er is iets niet goed gegaan. Probeer het opnieuw.", "OK");
                return;
            }
        }

        /// <summary>
        /// Function for trimming an API response to readable userid
        /// </summary>
        /// <param name="userid"></param>
        /// <returns>string userId</returns>
        private string TrimUserId(string userid)
        {
            // Trim userid to readable
            char[] charsToTrim = { '"', '\'' };
            userid = userid.Trim(charsToTrim);

            return userid;
        }

        /// <summary>
        /// Event handler for clicking CancelGoServers button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void CancelGoServersButtonClicked(object sender, EventArgs args)
        {
            // Go to Serverpage
            var nextPage = new ServerPage();
            await this.Navigation.PushModalAsync(nextPage);
        }

        /// <summary>
        /// Event handler for clicking GetApiInformation button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void GetApiInformationButtonClicked(object sender, EventArgs args)
        {
            // Go to Serverpage
            var nextPage = new ApiInformation();
            await this.Navigation.PushModalAsync(nextPage);
        }

        /// <summary>
        /// Function for logging an user out of application.
        /// </summary>
        /// <returns>Return await new page</returns>
        public async Task LogOutFromSystemAsync()
        {
            // Clear session storage from user
            Application.Current.Properties.Remove("userid");
            Application.Current.Properties.Remove("bearer"); 
            // Go to login page
            var nextPage = new MainPage();
            await this.Navigation.PushModalAsync(nextPage);
        }
    }
}