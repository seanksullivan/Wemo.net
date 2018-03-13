using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WemoNet.Communications;
using WemoNet.Responses;
using WemoNet.Utilities;
[assembly: InternalsVisibleTo("WemoNet.UnitTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010055d935950d773a8bf575cfee0b3b49a151a430bd7483fd46dc618c5992e8eb43a25f2778c61fb6775c95ae03c670a6b065ed421bc18e431c22a4048fbeb707423eaa2f0e194c49458cb8b155cce7c9950dcf30336ba83c24c6f826a1d17506e93279728dd1eac744f96174249f1067b3a52d56ab5d3b881cd608ea712663baf1")]
[assembly: InternalsVisibleTo("Communications.UnitTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010005ec78cd48383501b834ff539d47f3266bbd7597ec6df64116557c1dc928b262d0524653573960189e0d93a72cc9081a5da465d6cccf0cc155bc9a42fac7c1a972d25f1b927dd0ac55ab5972bc20b174d85178a76298f0d337f5163c4da668b1f2da5a45943193be96f909ed8c2e9aeefcdfd6e16a341209d71e35840dd2fbc2")]
[assembly: InternalsVisibleTo("WemoNet.IntegrationTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010005e473352172c301498ab91b360d1bcd528a9cc65954285523778c783987f00a9231245fa54e43d53d85f51d4fcdcac4e74a8adb92de5a04a396fa78cf969a6ca5a24136e92c1ca2d69038b15b655c49decf3e9c143b319dc4adf916fe36ead68dfdcd0b33ed400d28978bbf994d2e92bd6b214cec5a943d7ca6ec5a1d9955c8")]

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
