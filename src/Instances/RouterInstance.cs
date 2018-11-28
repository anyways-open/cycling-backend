using System;
using System.IO;
using Itinero;
using Itinero.LocalGeo;
using rideaway_backend.Extensions;
using rideaway_backend.Exceptions;
using rideaway_backend.FileMonitoring;
using Microsoft.Extensions.Configuration;
using rideaway_backend.Model;
using Serilog;

namespace rideaway_backend.Instance
{
    /// <summary>
    /// Static instance of the router.
    /// </summary>
    public static class RouterInstance
    {
        private static Router _router;
        private static RouterDb _routerDb;
        private static FileMonitor _monitor;

        /// <summary>
        /// Loads the routerdb into ram and starts the file monitor that automatically checks
        /// for updates in the routerdb and reloads when necessary.
        /// </summary>
        public static void Initialize(IConfiguration configuration)
        {
            var path = configuration.GetSection("Paths").GetValue<string>("RouterdbFile");
            using (var stream = new FileInfo(path).OpenRead())
            {
                _routerDb = RouterDb.Deserialize(stream);
            }

            _router = new Router(_routerDb);
            _monitor = new FileMonitor(path, TimeSpan.FromMinutes(1),
                () =>
                {
                    using (var stream = new FileInfo(path).OpenRead())
                    {
                        _routerDb = RouterDb.Deserialize(stream);

                        _router = new Router(_routerDb);
                        Log.Information("RouterDB has been updated to the latest version");
                    }
                });
        }

        /// <summary>
        /// Calculate a route.
        /// </summary>
        /// <param name="profileName">Name of the profile to use.</param>
        /// <param name="from">The starting coordinate.</param>
        /// <param name="to">The ending coordinate.</param>
        /// <returns>A Route object with a route between the two points</returns>
        /// <exception cref="ResolveException">
        /// Thrown when one of the points could not be resolved or when there is no path between the points.!--
        /// </exception>
        public static Route Calculate(string profileName, Coordinate from, Coordinate to)
        {
            Log.Information($"Handling request on profile {profileName} from {from} to {to}");
            if (string.IsNullOrWhiteSpace(profileName))
            {
                profileName = "bicycle";
            }
            else
            {
                profileName = "bicycle." + profileName;
            }

            if (!_router.Db.SupportProfile(profileName))
            {
                var profilesStr = "{";
                var profiles = _router.Db.GetSupportedProfiles();
                foreach (var p in profiles)
                {
                    profilesStr += p + ", ";
                }

                profilesStr = profilesStr.Substring(0, profilesStr.Length - 2) + "}";
                throw new ResolveException($"Profile not supported: {profileName}. Choose one of {profilesStr}");
            }

            var profile = _router.Db.GetSupportedProfile(profileName);
            var dist = 50;
            var point1 = _router.TryResolve(profile, from, dist);
            while (point1.IsError && dist < 1600)
            {
                dist *= 2;
                point1 = _router.TryResolve(profile, from, dist);
            }

            if (point1.IsError)
            {
                throw new ResolveException("Location 1 could not be resolved");
            }

            dist = 50;
            var point2 = _router.TryResolve(profile, to, dist);
            while (point2.IsError && dist < 1600)
            {
                dist *= 2;
                point2 = _router.TryResolve(profile, from, dist);
            }

            if (point2.IsError)
            {
                throw new ResolveException("Location 2 could not be resolved");
            }

            var result = _router.TryCalculate(profile, point1.Value, point2.Value);
            if (result.IsError)
            {
                throw new ResolveException("No path found between locations");
            }

            return result.Value;
        }

        /// <summary>
        /// Calculate a route.
        /// </summary>
        /// <param name="routeObj">Name of the profile to use.</param>
        /// <param name="language">The starting coordinate.</param>
        /// <returns>A GeoJsonFeatureCollection object representing the instructions.</returns>
        public static GeoJsonFeatureCollection GenerateInstructions(Route routeObj, string language = "en")
        {
            var rawInstructions = routeObj.GenerateInstructions(_routerDb, Languages.GetLanguage(language));
            rawInstructions = rawInstructions.makeContinuous(routeObj);
            rawInstructions = rawInstructions.simplify(routeObj);
            routeObj.CorrectColours(rawInstructions);
            return rawInstructions.ToGeoJsonCollection(routeObj);
        }
    }
}