using Communications.Responses;
using Communications.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Communications
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
        public HttpWebRequest WebRequest { get; set; }
        #endregion

        public WemoResponse GetResponse(Soap.WemoGetCommands cmd, string ipAddress)
        {
            WemoResponse response;

            //string IPADDR = "192.168.1.4";

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object, else create
            var req = WebRequest ?? HttpRequest.CreateHttpWebRequest($"{ipAddress}:{Port}{Event}", ContentType, SoapAction, cmd, RequestMethod);

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateGetRequest(cmd);

            // Write the Soap Request to the Request Stream
            using (var requestStream = req.GetRequestStream())
            {
                var encoding = new UTF8Encoding();
                requestStream.Write(encoding.GetBytes(reqContentSoap), 0, encoding.GetByteCount(reqContentSoap));
            }

            // Send the Request and acquire the Response
            try
            {
                var httpResponse = req.GetResponse() as HttpWebResponse;
                using (var rspStm = httpResponse.GetResponseStream())
                {
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

        public T GetResponseObject<T>(WemoResponse response)
        {
            // Soap parsing
            XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
            var doc = XDocument.Parse(response.ResponseBody).Descendants().Descendants(ns + "Body").FirstOrDefault().Descendants().FirstOrDefault();

            // Deserialize to the specific class
            var responseObject = SerializationUtil.Deserialize<T>(doc);
            return responseObject;
        }

        public string GetResponseValue(WemoResponse response)
        {
            var value = string.Empty;
            
            // Soap parsing
            XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
            value = XDocument.Parse(response.ResponseBody)
                .Descendants()
                .Descendants(ns + "Body").FirstOrDefault()
                .Descendants()
                .FirstOrDefault().Value;

            return value;
        }

        /// <summary>
        /// Temp
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ipAddress"></param>
        /// <param name="targetStatus"></param>
        /// <returns></returns>
        public bool SetBinaryState(Soap.WemoSetBinaryStateCommands cmd, string ipAddress, bool targetStatus)
        {
            bool success = false;
            var target = Convert.ToInt32(targetStatus);

            //string IPADDR = "192.168.1.4";

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object, else create
            var request = WebRequest ?? HttpRequest.CreateHttpWebRequest($"{ipAddress}:{Port}{Event}", ContentType, SoapAction, "SetBinaryState", RequestMethod);

            var response = GetResponse(cmd.ToString(), request, target.ToString());
            var responsObj = GetResponseObject<SetBinaryStateResponse>(response);

            if (responsObj.BinaryState == "0" || responsObj.BinaryState == "1")
            {
                success = true;
            }
            return success;
        }

        public WemoResponse SetBinaryState__(Soap.WemoSetBinaryStateCommands cmd, string ipAddress, string targetStatus)
        {
            WemoResponse response;

            //string IPADDR = "192.168.1.4";

            // Construct the HttpWebRequest - if not null we will use the supplied HttpWebRequest object, else create
            var req = WebRequest ?? HttpRequest.CreateHttpWebRequest($"{ipAddress}:{Port}{Event}", ContentType, SoapAction, "SetBinaryState", RequestMethod);

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateSetBinaryStateRequest(cmd.ToString(), targetStatus);

            // Write the Soap Request to the Request Stream
            using (var requestStream = req.GetRequestStream())
            {
                var encoding = new UTF8Encoding();
                requestStream.Write(encoding.GetBytes(reqContentSoap), 0, encoding.GetByteCount(reqContentSoap));
            }

            // Send the Request and acquire the Response
            try
            {
                var httpResponse = req.GetResponse() as HttpWebResponse;
                using (var rspStm = httpResponse.GetResponseStream())
                {
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

        private WemoResponse GetResponse(string cmd, HttpWebRequest request, string targetStatus)
        {
            WemoResponse response;

            // Construct the Soap Request
            var reqContentSoap = Soap.GenerateSetBinaryStateRequest(cmd, targetStatus);

            // Write the Soap Request to the Request Stream
            using (var requestStream = request.GetRequestStream())
            {
                var encoding = new UTF8Encoding();
                requestStream.Write(encoding.GetBytes(reqContentSoap), 0, encoding.GetByteCount(reqContentSoap));
            }

            // Send the Request and acquire the Response
            try
            {
                var httpResponse = request.GetResponse() as HttpWebResponse;
                using (var rspStm = httpResponse.GetResponseStream())
                {
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
    }
}
