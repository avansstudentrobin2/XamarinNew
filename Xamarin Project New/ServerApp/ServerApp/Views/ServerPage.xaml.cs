using Newtonsoft.Json;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ServerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerPage : ContentPage
    {
        private List<string> mItems;
        public ServerPage()
        {
            // If User is not logged in, go back to login page
            if(string.IsNullOrEmpty(Application.Current.Properties["userid"].ToString()) || string.IsNullOrEmpty(Application.Current.Properties["bearer"].ToString()))
            {
                LogOutFromSystemAsync();
            }
            InitializeComponent();
            GetServersAsync();
        }

        /// <summary>
        /// Event handler for clicking RegisterNewServer button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void RegisterNewServerButtonClicked(object sender, EventArgs args)
        {
            // Go to page register a new server
            var nextPage = new EnterServerPage();
            await this.Navigation.PushModalAsync(nextPage);
        }

        /// <summary>
        /// Function for loggin the user out of the system
        /// </summary>
        /// <returns></returns>
        private async Task LogOutFromSystemAsync()
        {
            // Clear session storage from user
            Application.Current.Properties.Remove("userid");
            Application.Current.Properties.Remove("bearer");
            // Go to login page
            var nextPage = new MainPage();
            await this.Navigation.PushModalAsync(nextPage);
        }

        /// <summary>
        /// Event handler for clicking Logout button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void LogoutButtonClicked(object sender, EventArgs args)
        {
            LogOutFromSystemAsync();
        }

        /// <summary>
        /// Function for getting a list of all the servers from an user. Set List to frontend
        /// </summary>
        private async void GetServersAsync()
        {
            // Get servers from API
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "api/Server/GetAllServers/" + Application.Current.Properties["userid"].ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Bearer " + Application.Current.Properties["bearer"].ToString());
            var response = await client.SendAsync(request);
            // Parse response with servers
            var responseString = await response.Content.ReadAsStringAsync();
            // Serialize to dynamic and push to Listview
            var responseDynamic = JsonConvert.DeserializeObject<List<Servers>>(responseString);
            foreach(Servers server in responseDynamic)
            {
                if(server.ServerIp == null)
                {
                    server.ServerIp = "(Default Server-IP)";
                }
            }
            listServers.ItemsSource = responseDynamic;
        }

        /// <summary>
        /// Event handler for clicking an server from Listview. Give selectedServer to new page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void SelectServerTabHandler(object sender, ItemTappedEventArgs args)
        {
            // Give selected server to new page
            Servers selectedServer = (Servers)args.Item;
            var nextPage = new ServerDetails(selectedServer);
            this.Navigation.PushModalAsync(nextPage);
        }
    }
}