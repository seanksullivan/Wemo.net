using System.Net;
using System.Threading.Tasks;
using WemoNet.Communications;
using WemoNet.Responses;
using WemoNet.Utilities;

namespace WemoNet
{
    public class Wemo
    {
        #region Public Properties - Test usage only

        /// <summary>
        /// TEST USAGE - specifically to provide a cheap-n-easy way to provide a Mock object
        /// </summary>
        public HttpWebRequest GetResponseWebRequest { get; set; }

        /// <summary>
        /// TEST USAGE - specifically to provide a cheap-n-easy way to provide a Mock object
        /// </summary>
        public HttpWebRequest SetResponseWebRequest { get; set; }
        #endregion

        /// <summary>
        /// Call the command to construct a soap Http Request - returning a translated response object 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<WemoResponse> GetWemoPlugResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };
            var response = await plug.GetResponseAsync(cmd, ipAddress);
            return response;
        }

        public async Task<T> GetWemoResponseObjectAsync<T>(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var objResponse = plug.GetResponseObject<T>(response);
            return (T)objResponse;
        }

        public async Task<string> GetWemoResponseValueAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var value = plug.GetResponseValue(response);
            return value;
        }

        public async Task<bool> ToggleWemoPlugAsync(Soap.WemoSetBinaryStateCommands cmd, string ipAddress)
        {
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
            var plug = new WemoPlug { WebRequest = SetResponseWebRequest };
            var response = await plug.SetBinaryStateAsync(cmd, ipAddress, binaryStateValue);
            return response;
        }

        public async Task<bool> TurnOnWemoPlugAsync(string ipAddress)
        {
            var success = await SetWemoPlugAsync(ipAddress, true);
            return success;
        }

        public async Task<bool> TurnOffWemoPlugAsync(string ipAddress)
        {
            var success = await SetWemoPlugAsync(ipAddress, false);
            return success;
        }

        private async Task<bool> SetWemoPlugAsync(string ipAddress, bool on)
        {
            bool success = true;

            var existingState = await GetWemoResponseObjectAsync<GetBinaryStateResponse>(Soap.WemoGetCommands.GetBinaryState, ipAddress);

            var plug = new WemoPlug { WebRequest = SetResponseWebRequest };
            if (on && existingState.BinaryState == "0")
            {
                success = await plug.SetBinaryStateAsync(Soap.WemoSetBinaryStateCommands.BinaryState, ipAddress, true);
            }

            if (!on && existingState.BinaryState == "1")
            {
                success = await plug.SetBinaryStateAsync(Soap.WemoSetBinaryStateCommands.BinaryState, ipAddress, false);
            }
            return success;
        }
    }

}
