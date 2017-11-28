using Communications;
using Communications.Utilities;
using Communications.Responses;
using System.Net;
using System;
using System.Threading.Tasks;

namespace WemoNet
{
    public class Wemo
    {
        #region Public Properties
        public HttpWebRequest WebRequest { get; set; }
        #endregion

        public async Task<WemoResponse> GetResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };
            var response = await wemo.GetResponseAsync(cmd, ipAddress);
            return response;
        }

        public async Task<T> GetResponseObjectAsync<T>(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var response = await wemo.GetResponseAsync(cmd, ipAddress);
            var objResponse = wemo.GetResponseObject<T>(response);
            return (T)objResponse;
        }
        public async Task<string> GetResponseValue(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var response = await wemo.GetResponseAsync(cmd, ipAddress);
            var value = wemo.GetResponseValue(response);
            return value;
        }

        public async Task<bool> ToggleSwitchAsync(Soap.WemoSetBinaryStateCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var existingState = await GetResponseObjectAsync<GetBinaryStateResponse>(Soap.WemoGetCommands.GetBinaryState, ipAddress);

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

            var response = await wemo.SetBinaryStateAsync(cmd, ipAddress, binaryStateValue);
            return response;
        }
    }

}
