using System;
using System.IO;
using System.Net;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using rideaway_backend.Instance;
using rideaway_backend.Util;
using Xunit;
using Xunit.Abstractions;

namespace Rideaway.Tests
{
    public class RouterInstanceTest
    {
        private readonly ITestOutputHelper _output;

        private static string path = "belgium.routerdb";

        public RouterInstanceTest(ITestOutputHelper output)
        {
            _output = output;
        }


        private void Log(string s)
        {
            _output.WriteLine(s);
        }

        /// <summary>
        ///  This test downloads belgium.osm.pbf and builds the router database (if it doesn't exist yet)
        /// </summary>
        [Fact]
        public void FixRouterDb()
        {
            if (File.Exists(path))
            {
                Log("Found the routerdb already.");
                return;
            }


            Log("Downloading routerdb...");
            var itineroDownloadsBe = new Uri("http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf");

            var fileReq = (HttpWebRequest) WebRequest.Create(itineroDownloadsBe);
            var fileResp = (HttpWebResponse) fileReq.GetResponse();
            using (var httpStream = fileResp.GetResponseStream())
            {
                using (var fileStream = File.Create("belgium.osm.pbf"))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    httpStream.CopyTo(fileStream);
                    fileStream.Close();
                }
            }

            using (var stream = File.OpenRead("belgium.osm.pbf"))
            {
                var routerDb = new RouterDb();
                Log("Stream successfully opened...");
                routerDb.LoadOsmData(stream, Vehicle.Bicycle);
                Log("Serializing...");
                using (var outStream = new FileInfo(path).Open(FileMode.Create))
                {
                    routerDb.Serialize(outStream);
                    Log("DONE!");
                }
            }
        }

        [Fact]
        public void TestProgressiveResolve()
        {
            FixRouterDb();
            RouterInstance.Initialize(path, TimeSpan.FromMinutes(1));


            var profile = RouterInstance.GetRouter().Db.GetSupportedProfile("bicycle");
            var point = RouterInstance.ResolvePointProgressive(profile,
                Utility.ParseCoordinate("50.96951708243853,5.482125931952595"));
            Assert.NotNull(point);

            // Middenin grote waterplas
            point = RouterInstance.ResolvePointProgressive(profile,
                Utility.ParseCoordinate("51.22806105459313,2.9527336278404164"));
            Assert.NotNull(point);


            // Paar kilometer in zee
            point = RouterInstance.ResolvePointProgressive(profile,
                Utility.ParseCoordinate("51.2905207937151,2.847190561776017"));
            Assert.NotNull(point);
        }
    }
}