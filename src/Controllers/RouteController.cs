using System;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using rideaway_backend.Exceptions;
using rideaway_backend.Extensions;
using rideaway_backend.Instance;
using rideaway_backend.Model;
using rideaway_backend.Util;
using Serilog;

namespace rideaway_backend.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controller for the routing endpoint.
    /// </summary>
    [Route("[controller]")]
    public class RouteController : Controller
    {
        
        
        /// <summary>
        /// Main endpoint for the application, is invoked by a GET-request to <c>hostname/route</c>.
        /// </summary>
        /// <param name="loc1">The starting point of the route.</param>
        /// <param name="loc2">The ending point of the route.</param>
        /// <param name="profile">The routing profile to use.</param>
        /// <param name="instructions">Return instructions or not.</param>
        /// <param name="lang">Language of the instructions.</param>
        /// <returns>JSON result with geoJSON featurecollection representing the route.</returns>
        [HttpGet]
        [EnableCors("AllowAnyOrigin")]
        public ActionResult Get(string loc1, string loc2, string profile = "", bool instructions = false,
            string lang = "en")
        {
            try
            {
                var from = Utility.ParseCoordinate(loc1);
                var to = Utility.ParseCoordinate(loc2);
                var route = RouterInstance.Calculate(profile, from, to);
                
                route.PruneColours();
                
                GeoJsonFeatureCollection instr = null;
                if (instructions)
                {
                    instr = RouterInstance.GenerateInstructions(route, lang);
                }

                RequestLogger.LogRequest(from, to);
                return Json(new RouteResponse(route, instr));
            }
            catch (ResolveException e)
            {
                Log.Error(e, "Getting a route failed (not found)");
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                Log.Error(e, "Getting a route failed (other error)");
                return BadRequest(e.Message);
            }
        }
    }
}