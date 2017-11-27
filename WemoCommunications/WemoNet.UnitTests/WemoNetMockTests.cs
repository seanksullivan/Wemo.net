using Communications.Responses;
using Communications.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace WemoNet.UnitTests
{
    [TestClass]
    public class WemoNetMockTests
    {
        [TestMethod]
        [DeploymentItem("TestData")]
        public void GetResponse_MockCommunications_Verify()
        {
            // ARRANGE
            var ipAddress = "http://192.168.1.4";
            // Acquire the soap/Xml data that we wish to supply within our mock'd HttpWebRequest and HttpWebResponse

            // Read Text directly instead of Bytes - so that our Xml comparison is easier (aka, BOM)
            var responseBytes = Encoding.UTF8.GetBytes(File.ReadAllText("TestData\\GetHomeInfoResponse.xml"));
            var wemo = new Wemo();

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)
            var mockRequest = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", responseBytes);


            // ACT

            // Minimal inversion of control: Set the WebRequest property to provide out own Mock'd HttpWebRequest/Response
            wemo.WebRequest = mockRequest;
            var result = wemo.GetResponse(Soap.WemoGetCommands.GetHomeInfo, ipAddress);
            var resultBodyXml = XDocument.Parse(result.ResponseBody);

            // ASSERT
            Assert.IsTrue(result.StatusCode == HttpStatusCode.NotModified.ToString(), "Expected Http StatusCode not returned");

            var expectedXml = XDocument.Parse(Encoding.UTF8.GetString(responseBytes));
            var xmlCompares = XNode.DeepEquals(expectedXml, resultBodyXml);

            Assert.IsTrue(xmlCompares, "Expected ResponseBody not returned");
        }


        private static HttpWebRequest CreateMockHttpWebRequest(HttpStatusCode httpStatusCode, string statusDescription, byte[] responseBytes)
        {
            var requestBytes = Encoding.ASCII.GetBytes("Blah Blah Blah");
            Stream requestStream = new MemoryStream();
            Stream responseStream = new MemoryStream();

            using (var memStream = new MemoryStream(requestBytes))
            {
                memStream.CopyTo(requestStream);
                requestStream.Position = 0;
            }

            using (var responseMemStream = new MemoryStream(responseBytes))
            {
                responseMemStream.CopyTo(responseStream);
                responseStream.Position = 0;
            }

            var response = new Mock<HttpWebResponse>(MockBehavior.Loose);
            response.Setup(c => c.StatusCode).Returns(httpStatusCode);
            response.Setup(c => c.GetResponseStream()).Returns(responseStream);
            response.Setup(c => c.StatusDescription).Returns(statusDescription);

            var request = new Mock<HttpWebRequest>();
            request.Setup(c => c.GetRequestStream()).Returns(requestStream);

            request.Setup(s => s.GetResponse()).Returns(response.Object);
            return request.Object;
        }
    }
}
