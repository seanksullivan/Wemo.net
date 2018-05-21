using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WemoNet.Responses;
using WemoNet.Utilities;
using System.Net.NetworkInformation;
using System.Collections.Concurrent;

namespace WemoNet.Communications

{
    /// <summary>
    /// 
    /// </summary>
    public class WemoPlug
    {
        #region Public Properties
        public string ContentType { get; set; } = "text/xml; charset=\"utf-8\"";
        public string SoapAction { get; set; } = "SOAPACTION:\"urn:Belkin:service:basicevent:1#";
        public string Event { get; set; } = "/upnp/control/basicevent1";
        public string RequestMethod { get; set; } = "POST";
        public string Port { get; set; } = "49153";
        internal HttpWebRequest GetResponseWebRequest { get; set; }
        internal HttpWebRequest SetResponseWebRequest { get; set; }
        #endregion

        public async Task<WemoResponse> GetResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            WemoResponse response;

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object - which is probably a Mock
            var request = GetResponseWebRequest
                ?? HttpRequest.CreateGetCommandHttpWebRequest($"{ipAddress}:{Port}{Event}", ContentType, SoapAction, cmd, RequestMethod);

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateGetRequest(cmd);
            response = await ExecuteGetResponseAsync(request, reqContentSoap);
            return response;
        }

        public T GetResponseObject<T>(WemoResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.ResponseBody))
            {
                throw new Exception($"StatusCode: {response.StatusCode}, Description: {response.Description}");
            }

            // Soap parsing
            XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
            var doc = XDocument.Parse(response.ResponseBody)
                .Descendants()
                    .Descendants(ns + "Body").FirstOrDefault()
                        .Descendants().FirstOrDefault();

            // Deserialize to the specific class
            var responseObject = SerializationUtil.Deserialize<T>(doc);
            return responseObject;
        }

        public string GetResponseValue(WemoResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.ResponseBody))
            {
                throw new Exception($"StatusCode: {response.StatusCode}, Description: {response.Description}");
            }

            var value = string.Empty;

            // Soap parsing
            XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
            value = XDocument.Parse(response.ResponseBody)
                .Descendants()
                    .Descendants(ns + "Body").FirstOrDefault()
                        .Descendants().FirstOrDefault().Value;

            return value;
        }

        public async Task<bool> SetBinaryStateAsync(Soap.WemoSetBinaryStateCommands cmd, string ipAddress, bool targetStatus)
        {
            bool success = false;
            var target = Convert.ToInt32(targetStatus);

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object, else create
            var request = SetResponseWebRequest
                ?? HttpRequest.CreateHttpWebRequest($"{ipAddress}:{Port}{Event}", ContentType, SoapAction, "SetBinaryState", RequestMethod);

            var response = await GetBinaryStateResponseAsync(cmd.ToString(), request, target.ToString());
            var responsObj = GetResponseObject<SetBinaryStateResponse>(response);

            if (responsObj.BinaryState == "0" || responsObj.BinaryState == "1")
            {
                success = true;
            }
            return success;
        }


        /// <summary>
        /// Searches from an IP range. e.g. http://192.168.1, from http://x.x.x.1 through http://x.x.x.255, attempting to locate Wemo devices.
        /// </summary>
        /// <param name="ipAddressSeed">http://192.168.1</param>
        /// <returns></returns>
        public ConcurrentDictionary<string, string> GetListOfLocalWemoDevices(string ipAddressSeed)
        {
            if (string.IsNullOrWhiteSpace(ipAddressSeed))
            {
                throw new Exception("The ipAddressSeed value is required!");
            }

            var numProcs = Environment.ProcessorCount;
            var concurrencyLevel = numProcs * 2;
            var wemoDevices = new ConcurrentDictionary<string, string>(concurrencyLevel, 300);

            Parallel.For(5, 255,
            seed =>
            {
                // Set the Ip Address
                var ipAddress = $"{ipAddressSeed}.{seed}";

                // Verify the IP address is valid
                var validIpAddress = IPAddress.TryParse(ipAddress.Replace("http://", ""), out IPAddress address);

                // if the Ip address is not valid, then skip and get outta here...
                if (!validIpAddress) return;

                // Attempt to communicate with the Wemo device at the set Ip Address
                // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object - which is probably a Mock
                var request = GetResponseWebRequest
                    ?? HttpRequest.CreateGetCommandHttpWebRequest($"{ipAddress}:{Port}{Event}", 
                    ContentType, SoapAction, Soap.WemoGetCommands.GetFriendlyName, RequestMethod);

                // Construct the Soap Request
                var reqContentSoap = Soap.GenerateGetRequest(Soap.WemoGetCommands.GetFriendlyName);
                var validWemoDevice = VerifyWemoDevice(request, reqContentSoap);

                // If we are not an actual Wemo device, then skip and get outta here...
                if (!validWemoDevice) return;

                var newRequest = GetResponseWebRequest
                    ?? HttpRequest.CreateGetCommandHttpWebRequest($"{ipAddress}:{Port}{Event}",
                    ContentType, SoapAction, Soap.WemoGetCommands.GetFriendlyName, RequestMethod);

                // Construct the Soap Request
                var response = ExecuteGetResponseAsync(newRequest, reqContentSoap).GetAwaiter().GetResult();

                // If the Ip Address is truly a Wemo device, then deserialize and add it to the list
                if (response.StatusCode != "UnknownError")
                {
                    var friendly = GetResponseObject<GetFriendlyNameResponse>(response);
                    wemoDevices.TryAdd(ipAddress, friendly.FriendlyName);
                }
            });

            return wemoDevices;
        }

        #region Private Methods
        private static async Task<WemoResponse> ExecuteGetResponseAsync(HttpWebRequest request, string reqContentSoap)
        {
            WemoResponse response;
            try
            {
                // Write the Soap Request to the Request Stream
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    var encoding = new UTF8Encoding();
                    requestStream.Write(encoding.GetBytes(reqContentSoap), 0, encoding.GetByteCount(reqContentSoap));

                    // Send the Request and acquire the Response
                    using (var httpResponse = await request.GetResponseAsync() as HttpWebResponse)
                    using (var rspStm = httpResponse.GetResponseStream())
                    using (var reader = new StreamReader(rspStm))
                    {
                        // Translate the Http Response to our own Response object
                        response = new WemoResponse
                        {
                            Description = httpResponse.StatusDescription,
                            StatusCode = httpResponse.StatusCode.ToString(),
                            ResponseBody = reader.ReadToEnd()
                        };
                    }
                }
            }
            catch (WebException ex)
            {
                response = new WemoResponse
                {
                    Description = $"Exception message: {ex.Message}",
                    StatusCode = ex.Status.ToString(),
                    ResponseBody = string.Empty
                };
            }

            return response;
        }

        private static bool VerifyWemoDevice(HttpWebRequest request, string reqContentSoap)
        {
            var response = false;
            try
            {
                // Write the Soap Request to the Request Stream
                using (var requestStream = request.GetRequestStream())
                {
                    var encoding = new UTF8Encoding();
                    requestStream.Write(encoding.GetBytes(reqContentSoap), 0, encoding.GetByteCount(reqContentSoap));

                    // Send the Request and acquire the Response
                    using (var httpResponse = request.GetResponse() as HttpWebResponse)
                    {
                        var headers = httpResponse.Headers;
                        var headerValues = headers.GetValues("X-User-Agent").ToList();

                        if (headerValues.Exists(x => x.Contains("redsonic")))
                        {
                            response = true;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                response = false;
            }

            return response;
        }

        private async Task<WemoResponse> GetBinaryStateResponseAsync(string cmd, HttpWebRequest request, string targetStatus)
        {
            WemoResponse response;

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateSetBinaryStateRequest(cmd, targetStatus);
            response = await ExecuteGetResponseAsync(request, reqContentSoap);
            return response;
        }

        #endregion
    }
}
