using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ServerApp.Constants
{
    internal static class Constants
    {
        public static string IPAddress = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        public static string ApiUrl = $"http://{IPAddress}:8082/";
    }
}
