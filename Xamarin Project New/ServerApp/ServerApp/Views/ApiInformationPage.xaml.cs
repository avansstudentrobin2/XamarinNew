using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ServerApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApiInformation : ContentPage
    {
        public ApiInformation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for clicking GoBackNewServerButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void GoBackNewServerButtonEventHandler(object sender, EventArgs args)
        {
            // Go to Serverpage
            var nextPage = new ServerPage();
            await this.Navigation.PushModalAsync(nextPage);
        }
    }
}