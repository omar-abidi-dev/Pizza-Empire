using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>Gameplay bonuses from owned plots: carry capacity, customer rate, queue length.</summary>
    public static class PlotEffects
    {
        private const int BaseCarry = 5;
        private const int CarryPerStorage = 3;
        private const int StartQueue = 3;       // short queue inside the starting plot
        private const int MaxQueue = 8;

        private static Customers.CustomerQueue _mainQueue;

        public static void Apply(IReadOnlyList<PlotSave> plots, HashSet<Vector2Int> owned)
        {
            int storage = 0, parking = 0, counters = 1;
            foreach (var p in plots)
            {
                if (p.Kind == PlotKind.Storage) storage++;
                else if (p.Kind == PlotKind.Parking) parking++;
                else if (p.Kind == PlotKind.Counter) counters++;
            }

            var player = GameObject.FindWithTag("Player");
            var stack = player != null ? player.GetComponent<Player.PlayerStack>() : null;
            if (stack != null) stack.SetMaxCapacity(BaseCarry + CarryPerStorage * storage);

            var spawner = Object.FindAnyObjectByType<Customers.CustomerSpawner>();
            if (spawner != null) spawner.SetRateMultiplier(1f + 0.3f * parking + 0.5f * (counters - 1));

            // The starting queue runs south; it can only grow once the plot in front is open.
            if (_mainQueue == null)
            {
                var q = GameObject.Find("CustomerQueue");
                _mainQueue = q != null ? q.GetComponent<Customers.CustomerQueue>() : null;
            }
            if (_mainQueue != null)
            {
                bool roomInFront = owned.Contains(Vector2Int.down);
                _mainQueue.SetMaxSize(roomInFront ? Mathf.Min(StartQueue + 1 + parking, MaxQueue) : StartQueue);
            }
        }
    }
}
