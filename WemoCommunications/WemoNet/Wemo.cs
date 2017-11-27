using Communications;
using Communications.Utilities;
using Communications.Responses;
using System.Net;
using System;

namespace WemoNet
{
    public class Wemo
    {
        #region Public Properties
        public HttpWebRequest WebRequest { get; set; }
        #endregion

        public WemoResponse GetResponse(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };
            var response = wemo.GetResponse(cmd, ipAddress);
            return response;
        }

        public T GetResponseObject<T>(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var response = wemo.GetResponse(cmd, ipAddress);
            var objResponse = wemo.GetResponseObject<T>(response);
            return (T)objResponse;
        }
        public string GetResponseValue(Soap.WemoGetCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var response = wemo.GetResponse(cmd, ipAddress);
            var value = wemo.GetResponseValue(response);
            return value;
        }

        //public WemoResponse Set(Soap.WemoSetBinaryStateCommands cmd, string ipAddress, string targetStatus)
        //{
        //    var wemo = new WemoPlug { WebRequest = this.WebRequest };
        //    var response = wemo.SetBinaryState(cmd, ipAddress, targetStatus);
        //    return response;
        //}

        public bool ToggleSwitch(Soap.WemoSetBinaryStateCommands cmd, string ipAddress)
        {
            var wemo = new WemoPlug { WebRequest = this.WebRequest };

            var existingState = GetResponseObject<GetBinaryStateResponse>(Soap.WemoGetCommands.GetBinaryState, ipAddress);
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

            var response = wemo.SetBinaryState(cmd, ipAddress, binaryStateValue);
            return response;
        }
    }

}
