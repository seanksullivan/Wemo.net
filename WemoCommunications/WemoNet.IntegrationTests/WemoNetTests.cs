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
        public void GetListOfLocalWemoDevices_Verify()
        {
            // ARRANGE
            var ipAddressSeed = "192.168.1";
            var wemo = new Wemo();

            // ACT
            var listOfDevicesFound = wemo.GetListOfLocalWemoDevices(ipAddressSeed);

            // ASSERT
            Assert.IsTrue(listOfDevicesFound.Count > 0, 
                $"Expected to locate at least one Wemo device - but nothing located with the supplied IpAddress seed of {ipAddressSeed}");

            foreach (var device in listOfDevicesFound)
            {
                Console.WriteLine($"IpAddress: {device.Key}, Name: {device.Value}");
            }   
        }

        [TestMethod]
        public void GetResponse_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoPlugResponseAsync(Soap.WemoGetCommands.GetHomeId, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue(result.StatusCode == "OK", "Expected Http StatusCode not returned");
        }

        [TestMethod]
        public async Task VerifyWemoDeviceExists()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = await wemo.GetWemoResponseObjectAsync<GetBinaryStateResponse>(ipAddress);

            // ASSERT
            Assert.IsTrue((result.BinaryState == "0" || result.BinaryState == "1"), "Expected Http StatusCode not returned");
        }

        [TestMethod]
        public void GetResponseObject_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.5";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoResponseObjectAsync<GetFriendlyNameResponse>(ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsNotNull(result.FriendlyName, "The expected type was not returned");
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
