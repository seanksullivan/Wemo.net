using System.Net;

namespace Communications.Utilities
{
    public class HttpRequest
    {
        public static HttpWebRequest CreateGetCommandHttpWebRequest(string ipAddress, string contentType, string soapAction, 
            Soap.WemoGetCommands cmd, string requestMethod)
        {
            var req = CreateHttpWebRequest(ipAddress, contentType, soapAction, cmd.ToString(), requestMethod);
            return req;
        }

        public static HttpWebRequest CreateSetBinaryStateCommandHttpWebRequest(string ipAddress, string contentType, string soapAction, 
            Soap.WemoSetBinaryStateCommands cmd, string requestMethod)
        {
            var req = CreateHttpWebRequest(ipAddress, contentType, soapAction, cmd.ToString(), requestMethod);
            return req;
        }

        public static HttpWebRequest CreateHttpWebRequest(string ipAddress, string contentType, string soapAction, string cmd, string requestMethod)
        {
            var req = HttpWebRequest.Create(ipAddress) as HttpWebRequest;

            req.ContentType = contentType;
            req.Headers.Add($"{soapAction}{cmd}\"");
            req.Method = requestMethod;
            return req;
        }
    }
}
