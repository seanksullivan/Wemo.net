using Communications.Responses;
using Communications.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Xml.Linq;

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
            var result = wemo.GetResponse(Soap.WemoGetCommands.GetHomeInfo, ipAddress);

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
    }
}
