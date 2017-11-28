using Communications.Responses;
using Communications.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace WemoNet.IntegrationTests
{
    [TestClass]
    public class WemoNetTests
    {
        [TestMethod]
        [DeploymentItem("TestData")]
        public void GetResponse_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.4";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoPlugResponseAsync(Soap.WemoGetCommands.GetWatchdogFile, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue(result.StatusCode == "OK", "Expected Http StatusCode not returned");
        }

        [TestMethod]
        [DeploymentItem("TestData")]
        public void GetResponseObject_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.4";
            var wemo = new Wemo();

            // ACT
            var result = wemo.GetWemoResponseObjectAsync<GetHomeInfoResponse>(Soap.WemoGetCommands.GetHomeInfo, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsNotNull(result.HomeInfo, "The expected type was not returned");
        }

        [TestMethod]
        [DeploymentItem("TestData")]
        public void SetBinaryState_On_Verify()
        {
            // ARRANGE
            var test = true;
            var wow = Convert.ToInt32(test);

            var ipAddress = "http://192.168.1.4";
            var binaryStateValue = "0";
            var wemo = new Wemo();

            // ACT
            var result = wemo.ToggleWemoPlugAsync(Soap.WemoSetBinaryStateCommands.BinaryState, ipAddress).GetAwaiter().GetResult();

            // ASSERT
            Assert.IsTrue(result, "The switch toggle command was not successful as expected");
        }
    }
}
