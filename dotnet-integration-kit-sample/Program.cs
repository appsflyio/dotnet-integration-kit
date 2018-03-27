using System;
using dotnet_integration_kit;
using Jose;
using Newtonsoft.Json;

namespace dotnet_integration_kit_sample
{
    class Program
    {
        public static void Main(string[] args)
        {
            AppInstance.AFConfig config = new AppInstance.AFConfig("https://microapps.appsfly.io", "338435433042832", "abb3f71c-a8cc-4f2a-90aa-23ac3771f5f7");
            AppInstance clearTrip = new AppInstance(config, "io.appsfly.msctpactivities");
            var obj = ((object)new
            {
                
            });
            //Console.WriteLine(clearTrip.execSync("fetch_cities", obj, "generic"));
            clearTrip.exec("fetch_cities", obj, "generic", (error, result) => {
                Console.WriteLine(error);
            });
        }
    }
}
