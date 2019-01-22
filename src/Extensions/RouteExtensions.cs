using System.Collections.Generic;
using Itinero;
using Itinero.Navigation.Instructions;

namespace rideaway_backend.Extensions
{
    public static class RouteExtensions
    {
        /// <summary>
        /// Introspects the colours of the route.
        /// If, on a segment, multiple colours can be chosen, then the most popular colour is chosen.
        /// </summary>
        /// <param name="route"></param>
        public static void PruneColours(this Route route)
        {
            var ambiguityDetected = false;
            var hist = new Dictionary<string, int>();
            foreach (var meta in route.ShapeMeta)
            {
                if (!meta.Attributes.TryGetValue("cyclecolour", out var colour)) continue;

                if (colour.Contains(","))
                {
                    ambiguityDetected = true;
                    continue;
                }

                if (!hist.ContainsKey(colour))
                {
                    hist[colour] = 0;
                }

                hist[colour] = hist[colour] + 1;
            }

            if (!ambiguityDetected)
            {
                return;
            }

            /* FUNCTION */
            int GetCount(string colour)
            {
                return hist.ContainsKey(colour) ? hist[colour] : 0;
            }

            // We have a histogram! Time to make choices
            foreach (var meta in route.ShapeMeta)
            {
                if (!meta.Attributes.TryGetValue("cyclecolour", out var colour)) continue;
                if (!colour.Contains(",")) continue;

                var possibleColours = colour.Split(",");
                var best = possibleColours[0];
                var bestCount = GetCount(best);
                for (int i = 1; i < possibleColours.Length; i++)
                {
                    var col = possibleColours[i];
                    var count = GetCount(col);
                    if (bestCount < count)
                    {
                        count = bestCount;
                        best = col;
                    }
                }

                meta.Attributes.AddOrReplace("cyclecolour", best);
            }
        }


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
    }
}