using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using ServerApp;
using ServerApp.Views;
using Xamarin.Forms;
using Xamarin.Forms.Mocks;
using Assert = NUnit.Framework.Assert;

namespace UnitTestServerAppProject
{
    /// <summary>
    /// In this Unit Test we want to test the following functions:
    /// GetUserId()
    /// </summary>
    [TestClass]
    public class RegisterPageUnitTest
    {
        public string bearer = "";
        [SetUp]
        public async System.Threading.Tasks.Task SetUpAsync()
        {
            MockForms.Init(Device.UWP);
            Application.Current = new App() { };
            bearer = await GetBearerTokenAsync();
        }

        public async System.Threading.Tasks.Task<string> GetBearerTokenAsync()
        {
            // Create list with username & password data
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("Username", "robin-tanis@live.nl"));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "password"));
            nvc.Add(new KeyValuePair<string, string>("Password", "Tester1!"));
            // Create page send to function
            var page = new MainPage();
            JsonValue response = await page.GetBearerTokenAsync(nvc);
            // Set bearer token in local storage
            Application.Current.Properties["bearer"] = response["access_token"];
            // Return bearer token
            return response["access_token"];
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GetUserId()
        {
            await SetUpAsync();
            MockForms.Init();
            // Make new mainpage, get function
            var page = new MainPage();
            var response = page.GetUserId("robin-tanis@live.nl").Result;
            // Serialize object
            var responseString = response.Content.ReadAsStringAsync();
            char[] charsToTrim = { '"', '\'' };
            string userid = responseString.Result.ToString().Trim(charsToTrim);
            // If response is the same as the ID in database
            Assert.AreEqual(userid, "e956f042-cf1e-44b5-8c5b-fc3c7cea8e27");
        }

    }
}
