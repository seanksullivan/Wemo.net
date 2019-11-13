using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WemoNet.Responses;
using WemoNet.Utilities;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace WemoNet.Communications

{
    /// <summary>
    /// 
    /// </summary>
    public class WemoPlug
    {
        #region Internal Properties
        internal string ContentType { get; set; } = "text/xml; charset=\"utf-8\"";
        internal string SoapAction { get; set; } = "SOAPACTION:\"urn:Belkin:service:basicevent:1#";
        internal string Event { get; set; } = "/upnp/control/basicevent1";
        internal HttpWebRequest Request { get; set; }
        internal HttpWebRequest BinarySetRequest { get; set; }
        #endregion

        public int Port { get; set; } = 49153;

        /// <summary>
        /// Default Ctor
        /// </summary>
        public WemoPlug()
        {

        }

        public WemoPlug(int port)
        {
            if (port != 0)
            {
                Port = port;
            }
        }

        public async Task<WemoResponse> GetResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            WemoResponse response;

            // Construct the HttpWebRequest
            var request = CreateHttpWebRequest(cmd, ipAddress);

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

            var request = CreateBinaryStateHttpWebRequest(Soap.WemoGetCommands.SetBinaryState, ipAddress);

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
        public async Task<ConcurrentDictionary<string, string>> GetListOfLocalWemoDevicesAsync(string ipAddressSeed)
        {
            if (string.IsNullOrWhiteSpace(ipAddressSeed))
            {
                throw new Exception("The ipAddressSeed value is required!");
            }

            var numProcs = Environment.ProcessorCount;
            var concurrencyLevel = numProcs * 2;
            var wemoDevices = new ConcurrentDictionary<string, string>(concurrencyLevel, 300);

            await Task.Run(() =>
            {
                Parallel.For(1, 255,
                async seed =>
                {
                    // Set the Ip Address
                    var ipAddress = $"{ipAddressSeed}.{seed}";

                    // Verify the IP address is valid
                    var validIpAddress = IPAddress.TryParse(ipAddress.Replace("http://", ""), out IPAddress address);

                    // if the Ip address is not valid, then skip and get outta here...
                    if (!validIpAddress) return;

                    // Attempt to communicate with the Wemo device at the set Ip Address
                    var request = CreateHttpWebRequest(Soap.WemoGetCommands.GetFriendlyName, ipAddress);

                    // Construct the Soap Request
                    var reqContentSoap = Soap.GenerateGetRequest(Soap.WemoGetCommands.GetFriendlyName);

                    // Verify Wemo Device
                    var validWemoDevice = VerifyWemoDevice(request, reqContentSoap);

                    // If we are not an actual Wemo device, then skip and get outta here...
                    if (!validWemoDevice) return;

                    // Attempt to communicate with the verified Wemo device - we need to use a new Request object
                    var newRequest = CreateHttpWebRequest(Soap.WemoGetCommands.GetFriendlyName, ipAddress);
                    var response = await ExecuteGetResponseAsync(newRequest, reqContentSoap);

                    // If the Ip Address is truly a Wemo device, then deserialize and add it to the list
                    if (response.StatusCode != "UnknownError")
                    {
                        var friendly = GetResponseObject<GetFriendlyNameResponse>(response);
                        wemoDevices.TryAdd(ipAddress, friendly.FriendlyName);
                    }
                });
            });

            return wemoDevices;
        }

        /// <summary>
        /// Check to see if a given IP address is for a Wemo device.
        /// </summary>
        /// <param name="ipAddress">e.g. http://192.168.1.101</param>
        /// <returns>A KeyValuePair<string,string> populated with the ipaddress and friendly name if an existing Wemo device</string> object .</returns>
        public async Task<KeyValuePair<string,string>> IsLocalWemoDeviceAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new Exception("The ipAddressSeed value is required!");
            }

            KeyValuePair<string,string> keyValuePair;

            // Verify the IP address is valid
            var validIpAddress = IPAddress.TryParse(ipAddress.Replace("http://", ""), out IPAddress address);

            // if the Ip address is not valid, then skip and get outta here...
            if (!validIpAddress) return new KeyValuePair<string, string>(ipAddress,null);

            // Attempt to communicate with the Wemo device at the set Ip Address
            var request = CreateHttpWebRequest(Soap.WemoGetCommands.GetFriendlyName, ipAddress);

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateGetRequest(Soap.WemoGetCommands.GetFriendlyName);

            // Verify Wemo Device
            var validWemoDevice = VerifyWemoDevice(request, reqContentSoap);

            // If we are not an actual Wemo device, then skip and get outta here...
            if (!validWemoDevice) return new KeyValuePair<string, string>(ipAddress, null);

            // Attempt to communicate with the verified Wemo device - we need to use a new Request object
            var newRequest = CreateHttpWebRequest(Soap.WemoGetCommands.GetFriendlyName, ipAddress);
            var response = await ExecuteGetResponseAsync(newRequest, reqContentSoap);

            // If the Ip Address is truly a Wemo device, then deserialize and add it to the list
            if (response.StatusCode != "UnknownError")
            {
                var friendly = GetResponseObject<GetFriendlyNameResponse>(response);

                //wemoDevices.TryAdd(ipAddress, friendly.FriendlyName);
                keyValuePair = new KeyValuePair<string, string>(ipAddress, friendly.FriendlyName);
            }
            else
            {
                new KeyValuePair<string, string>(ipAddress, null);
            }

            return keyValuePair;
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

        private async Task<string> InvokeRestAsync(string baseAddress, string url)
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("HOST", baseAddress);

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    result = response.StatusCode.ToString();
                }
            }
            return result;
        }

        private HttpWebRequest CreateHttpWebRequest(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var fullIpAddress = $"{ipAddress}:{Port}{Event}";

            HttpWebRequest request = Request;
            if (request == null)
            {
                request = HttpRequest.CreateHttpWebRequest(fullIpAddress, ContentType, SoapAction, cmd.ToString(), "POST");
            }
            return request;
        }

        private HttpWebRequest CreateBinaryStateHttpWebRequest(Soap.WemoGetCommands cmd, string ipAddress)
        {
            HttpWebRequest request = BinarySetRequest;
            if (request == null)
            {
                request = CreateHttpWebRequest(cmd, ipAddress);
            }
            return request;
        }
        #endregion
    }
}
