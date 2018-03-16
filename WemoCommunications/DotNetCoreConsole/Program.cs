using System;
using System.Linq;
using WemoNet;

namespace DotNetCoreConsole
{
    class Program
    {
        /// <summary>
        /// Run the console app and pass-in a valid IP Address.  e.g.: "http://192.168.1.5"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Toggle a swith for a given IP address
            var ipAddress = args.FirstOrDefault();

            // Verify the ipAddress is an actual ip address
            var uriConvertSuccess = Uri.TryCreate(ipAddress, UriKind.Absolute, out Uri uri);

            if (!uriConvertSuccess)
            {
                Console.WriteLine("Not a valid Ip Address");
                return;
            }

            var wemo = new Wemo();

            // ACT
            var result = wemo.ToggleWemoPlugAsync(ipAddress).GetAwaiter().GetResult();
        }
    }
}
