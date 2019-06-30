using Newtonsoft.Json;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Entry = Microcharts.Entry;
using SkiaSharp;
using Microcharts;

namespace ServerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerDetails : ContentPage
    {
        List<Entry> entries = new List<Entry>();
        private decimal usedMemory = 0;
        private decimal serverMemory = 0;

        public ServerDetails(Servers server)
        {
            InitializeComponent();
            // Set all the xaml text fields
            SetXamlFields(server);
            // Set checks, set line diagram with data from your server
            SetServerDataAsync(server);
        }

        /// <summary>
        /// Function for setting all the data from the server on the page
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Void Task</returns>
        private async Task SetServerDataAsync(Servers server)
        {
            // If server != default, check if server is valid server
            HttpResponseMessage isValid = new HttpResponseMessage();
            try
            {
                if (server.ServerName.ToLower() != "default")
                {
                    isValid = await CheckServerAsync(server);
                }
            }
            catch
            {
                // Error return to serverpage
                await ErrorHandlingAsync("De server is op dit moment niet beschikbaar. Probeer het opnieuw.", server.Id);
            }
            // If server is valid or default
            if (server.ServerName.ToLower() == "default" || isValid.IsSuccessStatusCode)
            {
                // Update every 3 seconds, call function UpdateServerAsync()
                var timer = new System.Threading.Timer(
                    e => UpdateServerDefaultServerAsync(server),
                    null,
                    TimeSpan.Zero,
                    TimeSpan.FromSeconds(3));
            }
            else
            {
                // Error, return to Serverpage. 
                await ErrorHandlingAsync("De server is op dit moment niet beschikbaar. Probeer het opnieuw.", server.Id);
            }
        }

        /// <summary>
        /// Function for setting the frontend fields with serverdata. 
        /// </summary>
        /// <param name="server"></param>
        private void SetXamlFields(Servers server)
        {
            // Set fields in xaml file
            serverName.Text = "Servernaam: " + server.ServerName;
            serverIp.Text = "Server URL: " + server.ServerIp;
        }

        /// <summary>
        /// Function for getting the monitoring information about the server. 
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Serverdata</returns>
        public async Task<HttpResponseMessage> CheckServerAsync(Servers server)
        {
            // Get serverdata
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            string url = "";
            HttpRequestMessage request = new HttpRequestMessage();
            url = server.ServerIp + "/" + server.Password;
            request = new HttpRequestMessage(HttpMethod.Get, url);
            // Build request for checking the server
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDynamic = JsonConvert.DeserializeObject<List<string>>(responseString);
            decimal result;
            // Check if user has correct password
            if(!Decimal.TryParse(responseDynamic[0], out result))
            {
                await ErrorHandlingAsync("Het meegegeven wachtwoord is onjuist. Probeer het opnieuw.", server.Id);
            }
            return response;
        }

        /// <summary>
        /// Function for handling all the errors from API in this page. Ask user for deleting server.
        /// </summary>
        /// <returns></returns>
        private async Task ErrorHandlingAsync(string message, int serverId)
        {
            // Error, password incorrect. Return to Serverpage. 
            var nextPage = new ServerPage();
            await this.Navigation.PushModalAsync(nextPage);
            // Check next action from user. Delete or do nothing
            string nextAction = " Wat wilt u met de server doen?";
            bool wantToDeleteServer = await DisplayAlert("Fout", message + nextAction, "Verwijder server", "Bewaar de server");
            // If user want to delete the server, delete it
            if (wantToDeleteServer)
            {
                // Double check user want to delete server
                bool sureWantToDeleteServer = await DisplayAlert("Let op", "Weet u zeker dat u de server wilt verwijderen?", "JA", "NEE");
                if (sureWantToDeleteServer)
                {
                    await DeleteServerAsync(serverId);
                }
            }
        }

        /// <summary>
        /// Function for deleting the selected server
        /// </summary>
        /// <param name="serverId"></param>
        private async Task DeleteServerAsync(int serverId)
        {
            // Delete server where Servers.Id == serverId
            var client = new HttpClient();
            string url = Constants.Constants.ApiUrl + "api/Server/DeleteServer/" + serverId.ToString();
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Authorization", "Bearer " + Application.Current.Properties["bearer"].ToString());
            var response = await client.SendAsync(request);
            // Server with success deleted. Show message
            if(response.IsSuccessStatusCode)
            {
                var nextPage = new ServerPage();
                await this.Navigation.PushModalAsync(nextPage);
                await DisplayAlert("Geslaagd", "De server is met succes verwijderd.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "De server niet verwijderd. Probeer het opnieuw.", "OK");
            }
        }

        /// <summary>
        /// Function for update the server status for monitoring.
        /// </summary>
        /// <param name="server"></param>
        /// <returns>API monitoring data</returns>
        private async Task UpdateServerDefaultServerAsync(Servers server)
        {
            try
            {
                // Get serverdata
                var client = new HttpClient();
                string url = "";
                HttpRequestMessage request = new HttpRequestMessage();
                // If server == default, add bearer token & default url
                if (server.ServerName.ToLower() == "default")
                {
                    url = Constants.Constants.ApiUrl + "api/Server/GetHealth";
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Authorization", "Bearer " + Application.Current.Properties["bearer"].ToString());
                }
                else
                {
                    url = server.ServerIp + "/" + server.Password;
                    request = new HttpRequestMessage(HttpMethod.Get, url);
                }
                var response = await client.SendAsync(request);
                // If call is successed
                if (response.IsSuccessStatusCode)
                {
                    SerializeDataMonitoringAsync(response);
                } 
                else
                {
                    // Error, return to Serverpage. 
                    await ErrorHandlingAsync("De gegevens van de server worden niet meer bijgehouden. Probeer het opnieuw.", server.Id);
                }
            } catch
            {
                // Error, return to Serverpage. 
                await ErrorHandlingAsync("De server is op dit moment niet beschikbaar. Probeer het opnieuw.", server.Id);
            }
        }

        /// <summary>
        /// Function for serializing data and making an graphic overview.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Server monitoring data</returns>
        private async Task SerializeDataMonitoringAsync(HttpResponseMessage response)
        {
            // Serialize response
            var responseString = await response.Content.ReadAsStringAsync();
            var responseDynamic = JsonConvert.DeserializeObject<List<string>>(responseString);
            // Parse values to decimals. Api give 3 values back. We use 2
            serverMemory = decimal.Parse(responseDynamic[0].Replace(",", "."));
            usedMemory = decimal.Parse(responseDynamic[1].Replace(",", "."));
            // Add entry with new server data
            entries.Add(
                new Entry((float)usedMemory)
                {
                    Color = SKColor.Parse("#00CED1"),
                    Label = DateTime.Now.ToLongTimeString(),
                    ValueLabel = usedMemory.ToString()
                });
            // If more then 12 entry's are loaded, delete the first one
            if (entries.Count == 12)
            {
                entries.RemoveAt(0);
            }
            // Reload the chartview
            chartView.Chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Spline,
                MaxValue = (int)Math.Ceiling(usedMemory),
                MinValue = (int)Math.Floor(usedMemory)
            };
        }
        
        /// <summary>
        /// Event handler for clicking GoBack button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void GoBackButtonClicked(object sender, EventArgs args)
        {
            // Go to Serverpage
            var nextPage = new ServerPage();
            await this.Navigation.PushModalAsync(nextPage);
        }
    }
}