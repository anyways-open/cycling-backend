using System.IO;
using System;
using Itinero.LocalGeo;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace rideaway_backend.Instance {
    /// <summary>
    /// Logger for requests of routes over the cyclenetwork.
    /// </summary>
    public static class RequestLogger {

        private static string path;

        /// <summary>
        /// Log request in file of current day with current timestamp.
        /// </summary>
        /// <param name="from">starting coordinate</param>
        /// <param name="to">ending coordinate</param>
        public static void LogRequest(Coordinate from, Coordinate to){
            var row = DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") + "," 
            + from.Latitude.ToString(new CultureInfo ("en-US")) + "," 
            + from.Longitude.ToString(new CultureInfo ("en-US")) + "," 
            + to.Latitude.ToString(new CultureInfo ("en-US")) + "," 
            + to.Longitude.ToString(new CultureInfo ("en-US")) + "\n";

            
            var filePath = path+ "/" + DateTime.Now.ToString("MMMM-yyyy") + ".csv";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.AppendAllText(filePath, row);
        }

        public static void initialize(IConfiguration configuration){
            path =  Path.Combine(@"wwwroot", 
            configuration.GetSection("Paths").GetValue<string>("LoggingEndpoint"),
            configuration.GetSection("Paths").GetValue<string>("LoggingFilesSubfolder"));
        }
    }
}