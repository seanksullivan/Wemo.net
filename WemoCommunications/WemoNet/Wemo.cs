using Communications;
using Communications.Utilities;
using Communications.Responses;
using System.Net;
using System.Threading.Tasks;

namespace WemoNet
{
    public class Wemo
    {
        #region Public Properties
        public HttpWebRequest WebRequest { get; set; }
        #endregion

        /// <summary>
        /// Call the command to construct a soap Http Request - returning a translated response object 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<WemoResponse> GetWemoPlugResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = this.WebRequest };
            var response = await plug.GetResponseAsync(cmd, ipAddress);
            return response;
        }

        public async Task<T> GetWemoResponseObjectAsync<T>(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = this.WebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var objResponse = plug.GetResponseObject<T>(response);
            return (T)objResponse;
        }

        public async Task<string> GetWemoResponseValueAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = this.WebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var value = plug.GetResponseValue(response);
            return value;
        }

        public async Task<bool> ToggleWemoPlugAsync(Soap.WemoSetBinaryStateCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = this.WebRequest };

            var existingState = await GetWemoResponseObjectAsync<GetBinaryStateResponse>(Soap.WemoGetCommands.GetBinaryState, ipAddress);

            var binaryStateValue = false;
            switch (existingState.BinaryState)
            {
                case "0":
                    binaryStateValue = true;
                    break;

                case "1":
                    binaryStateValue = false;
                    break;
            }

            var response = await plug.SetBinaryStateAsync(cmd, ipAddress, binaryStateValue);
            return response;
        }
    }

}
