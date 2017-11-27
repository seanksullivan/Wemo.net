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
            var result = wemo.GetResponse(Soap.WemoGetCommands.GetWatchdogFile, ipAddress);

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
            var result = wemo.GetResponseObject<GetHomeInfoResponse>(Soap.WemoGetCommands.GetHomeInfo, ipAddress);

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
            var response = wemo.GetResponseObject<GetBinaryStateResponse>(Soap.WemoGetCommands.GetBinaryState, ipAddress);
            switch (response.BinaryState)
            {
                case "0":
                    binaryStateValue = "1";
                    break;

                case "1":
                    binaryStateValue = "0";
                    break;
            }
            var result = wemo.ToggleSwitch(Soap.WemoSetBinaryStateCommands.BinaryState, ipAddress);

            // ASSERT
            Assert.IsTrue(result, "The switch toggle command was not successful as expected");
        }
    }
}
