using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using ServerApp;
using Xamarin.Forms.Mocks;
using System.Json;
using Assert = NUnit.Framework.Assert;
using Xamarin.Forms;


namespace UnitTestServerAppProject
{
    /// <summary>
    /// In this Unit Test we want to test the following functions:
    /// CheckServersAsync()
    /// GetUserId()
    /// GetBearerTokenAsync()
    /// </summary>
    [TestClass]
    public class MainPageUnitTest
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
        public async System.Threading.Tasks.Task GetBearerTokenTestAsync()
        {
            await SetUpAsync();
            MockForms.Init();
            // Call function for checking bearer token
            string localBearer = await GetBearerTokenAsync();
            // If bearer token > 30 = success
            Assert.IsTrue(localBearer.Length > 30);
            Application.Current.Properties.Remove("bearer");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task CheckServersMethodAsync()
        {
            await SetUpAsync();
            MockForms.Init();
            // Make new mainpage, get function
            var page = new MainPage();
            var response = page.CheckServersAsync("e956f042-cf1e-44b5-8c5b-fc3c7cea8e27").Result;
            // If response == true or false, test passed
            Assert.IsNotEmpty(response);
            //Assert.IsTrue(string.Equals(response.ToString(), "true") || string.Equals(response.ToString(), "false"));
            Application.Current.Properties.Remove("bearer");
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
            Application.Current.Properties.Remove("bearer");
        }
    }
}
