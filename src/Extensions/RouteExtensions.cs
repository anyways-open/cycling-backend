using System.Collections.Generic;
using System.Linq;
using Itinero;
using Itinero.Navigation.Instructions;

namespace rideaway_backend.Extensions
{
    public static class RouteExtensions
    {
        /// <summary>
        /// Correct the colours of the cyclenetwork on the route so there is only one colour
        /// present that matches the colours in the instructions
        /// </summary>
        /// <param name="Route">The route object</param>
        /// <param name="instructions">The list of instructions to get the colours from</param>
        public static void CorrectColours(this Route Route, IList<Instruction> instructions)
        {
            var instructionIndex = 0;
            var currentInstruction = instructions[instructionIndex];

            for (var i = 0; i < Route.ShapeMeta.Length; i++)
            {
                var currentShape = Route.ShapeMeta[i].Shape;
                if (currentShape == currentInstruction.Shape)
                {
                    instructionIndex++;
                    if (instructionIndex < instructions.Count - 1)
                    {
                        currentInstruction = instructions[instructionIndex];
                    }
                }

                if (i < Route.ShapeMeta.Length - 1)
                {
                    Route.ShapeMeta[i + 1].Attributes
                        .AddOrReplace("colour", currentInstruction.GetAttribute("cyclecolour", Route));
                }
            }
        }


        /// <summary>
        /// Creates a new ShapeMeta list within the route.
        /// This shapeMeta merges all the shapeMeta's with the same colour.
        ///
        /// Modifies the route
        /// </summary>
        public static void MergePerColour(this Route route)
        {
            var newShapeMeta = new List<Route.Meta>();


            var meta = route.ShapeMeta;

            
            for (int i = 0; i < meta.Length - 1; i++)
            {
                var attr = meta[i].Attributes;
                attr.TryGetValue("colour", out var colour);
                meta[i + 1].Attributes.TryGetValue("colour", out var nextColour);

                if (!Equals(colour, nextColour))
                {
                    attr.RemoveKey("name");
                    attr.RemoveKey("highway");

                    newShapeMeta.Add(meta[i]);
                }
            }

            var last = meta.Last().Attributes;
            last.RemoveKey("name");
            last.RemoveKey("highway");

            newShapeMeta.Add(meta.Last());


            route.ShapeMeta = newShapeMeta.ToArray();
        }
    }
}