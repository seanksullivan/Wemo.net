using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WemoNet.Communications;
using WemoNet.Responses;
using WemoNet.Utilities;
[assembly: InternalsVisibleTo("WemoNet.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b1ce739465d11659f2f7cb07256b3e41dcbd6ca6aa91dac277a4349e8dfce3fd1ad6607ff6af636c216b03ee70270ec9d91cb5e87c889a474d882526f42fef6b30191b7fcee34eb4bf3cdae9b12bccbf4437e45176e15a294bef11f9852a22629bd0f6dd33ae7e626490144bbe8377e87f8ddc86101c46674c69fb6e0d07e3ce")]
[assembly: InternalsVisibleTo("Communications.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a122e1687a210deced4a713a67d0e8c4bfc880eca1859c59a05b3fda0b0a58b9920f85dd9b5b97ed229387b4d04fdb9216e3d69ae4962a611e6f8cee64c880306d01cf67ef06de2ec6f8d63d739474bca308befe91afb79a6ad8640b7305fa5c98c714ebba9bf5ea7bd83eb6a0db7ee70c14ce980331a3e37950de8c794f49d3")]
[assembly: InternalsVisibleTo("WemoNet.IntegrationTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a114c92952923210a74e8fe0c2c3b27beb70405cb77b04dbe5d8fd35606c5a8daba33145d65d7e5a179d44777ea039f3c7efcd83ad6c957bc5a1ca89019df2afe9c84f3a05757e2e48d465dc9b308e130f9a1c926dc21a2b11fa2c03ea4681cbeebdf7ab4081c85f8f05abf8f38752d2b4e62d5c0b1d926c627cadf53f002ece")]

namespace WemoNet
{
    public class Wemo
    {
        #region Public Properties - Test usage only
        /// <summary>
        /// TEST USAGE - specifically to provide a cheap-n-easy way to provide a Mock object
        /// </summary>
        internal HttpWebRequest GetResponseWebRequest { get; set; }

        /// <summary>
        /// TEST USAGE - specifically to provide a cheap-n-easy way to provide a Mock object
        /// </summary>
        internal HttpWebRequest SetResponseWebRequest { get; set; }
        #endregion

        /// <summary>
        /// Call the command to construct a soap Http Request - returning a translated response object 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        internal async Task<WemoResponse> GetWemoPlugResponseAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };
            var response = await plug.GetResponseAsync(cmd, ipAddress);
            return response;
        }

        /// <summary>
        /// Acquire the existing state of the target Wemo plug.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        internal async Task<T> GetWemoResponseObjectAsync<T>(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var objResponse = plug.GetResponseObject<T>(response);
            return (T)objResponse;
        }

        internal async Task<string> GetWemoResponseValueAsync(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var plug = new WemoPlug { WebRequest = GetResponseWebRequest };

            var response = await plug.GetResponseAsync(cmd, ipAddress);
            var value = plug.GetResponseValue(response);
            return value;
        }

        /// <summary>
        /// Toggle the target Wemo plug - off or on. If the switch is currently off, it will be enabled, vice-versa.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<bool> ToggleWemoPlugAsync(string ipAddress)
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
            var response = await plug.SetBinaryStateAsync(Soap.WemoSetBinaryStateCommands.BinaryState, ipAddress, binaryStateValue);
            return response;
        }

        /// <summary>
        /// Enable the target Wemo plug - turn it on.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<bool> TurnOnWemoPlugAsync(string ipAddress)
        {
            var success = await SetWemoPlugAsync(ipAddress, true);
            return success;
        }

        /// <summary>
        /// Disable the target Wemo plug - turn it off.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public async Task<bool> TurnOffWemoPlugAsync(string ipAddress)
        {
            var success = await SetWemoPlugAsync(ipAddress, false);
            return success;
        }

        /// <summary>
        /// Enable or disable (turn on or off) the target Wemo plug.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="on">Turn on = true, Turn off = false</param>
        /// <returns></returns>
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
