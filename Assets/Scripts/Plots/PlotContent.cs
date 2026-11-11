using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// Weighted random table that decides what a newly bought plot becomes, plus the
    /// parameter it needs (which food the ovens cook, which worker the staff room hires).
    /// </summary>
    public static class PlotContent
    {
        public static string DisplayName(PlotKind kind) => kind switch
        {
            PlotKind.Ovens => "More Ovens",
            PlotKind.Dining => "Dining Area",
            PlotKind.DriveThru => "Drive-Thru Lane",
            PlotKind.Storage => "Packing Station",
            PlotKind.Counter => "Second Counter",
            PlotKind.Garden => "Garden",
            PlotKind.StaffRoom => "Staff Room",
            PlotKind.Parking => "Parking Lot",
            PlotKind.Kitchen => "New Kitchen",
            _ => "Kitchen"
        };

        /// <summary>Roll the content for the next plot given what is already owned.</summary>
        public static PlotSave Roll(Vector2Int cell, IReadOnlyList<PlotSave> owned)
        {
            int Count(PlotKind k) { int n = 0; foreach (var p in owned) if (p.Kind == k) n++; return n; }

            int counters = 1 + Count(PlotKind.Counter);
            int stations = 1 + 2 * (Count(PlotKind.Ovens) + Count(PlotKind.Kitchen));
            var table = new List<(PlotKind kind, float weight)>
            {
                (PlotKind.Ovens, stations < 3 * counters + 2 ? 3f : 1.5f),
                (PlotKind.Dining, 3f),
                (PlotKind.DriveThru, Count(PlotKind.DriveThru) == 0 && owned.Count >= 2 ? 2f : 0f),
                (PlotKind.Storage, Count(PlotKind.Storage) < 4 ? 2f : 0f),
                (PlotKind.Counter, counters < 4 && owned.Count >= 3 ? 1.5f : 0f),
                (PlotKind.Garden, 1.5f),
                (PlotKind.StaffRoom, 2.5f),
                (PlotKind.Parking, 1.5f),
            };

            float total = 0f;
            foreach (var e in table) total += e.weight;
            float roll = Random.value * total;
            PlotKind pick = PlotKind.Garden;
            foreach (var e in table)
            {
                if (e.weight <= 0f) continue;
                pick = e.kind;
                roll -= e.weight;
                if (roll <= 0f) break;
            }

            var save = new PlotSave { x = cell.x, y = cell.y, kind = (int)pick };
            if (pick == PlotKind.Ovens) save.param = LeastCookedFood(owned);
            if (pick == PlotKind.StaffRoom) save.param = (int)WorkerHiring.NextHire();
            return save;
        }

        /// <summary>The menu food with the fewest stations, so new ovens balance production.</summary>
        private static int LeastCookedFood(IReadOnlyList<PlotSave> owned)
        {
            var counts = new int[Kitchen.Menu.Count];
            counts[0] = 1; // the starting oven
            foreach (var p in owned)
                if ((p.Kind == PlotKind.Ovens || p.Kind == PlotKind.Kitchen) && p.param < counts.Length)
                    counts[p.param] += 2;
            int best = 0;
            for (int t = 1; t < counts.Length; t++) if (counts[t] < counts[best]) best = t;
            return best;
        }
    }
}
