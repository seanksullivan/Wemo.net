using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WemoNet.Responses;
using WemoNet.Utilities;

namespace WemoNet.IntegrationTests
{
    [TestClass]
    public class WemoNetTests
    {
        [TestMethod]
        public void GetResponse_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoPlugResponseAsync(Soap.WemoGetCommands.GetWatchdogFile, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue(result.StatusCode == "OK", "Expected Http StatusCode not returned");
        }

        [TestMethod]
        public void GetResponseObject_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoResponseObjectAsync<GetHomeInfoResponse>(Soap.WemoGetCommands.GetHomeInfo, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsNotNull(result.HomeInfo, "The expected type was not returned");
        }

        [TestMethod]
        public void ToggleWemoPlugAsync_Verify()
        {
            // ARRANGE
            var test = true;
            var wow = Convert.ToInt32(test);

            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = wemo.ToggleWemoPlugAsync(ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue(result, "The switch toggle command was not successful as expected");
        }

        [TestMethod]
        public async Task TurnOnWemoPlug_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = await wemo.TurnOnWemoPlugAsync(ipAddress);

            // ASSERT
            Assert.IsTrue(result, "The switch toggle command was not successful as expected");
        }

        [TestMethod]
        public async Task TurnOffWemoPlug_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = await wemo.TurnOffWemoPlugAsync(ipAddress);

            // ASSERT
            Assert.IsTrue(result, "The switch toggle command was not successful as expected");
        }
    }
}
