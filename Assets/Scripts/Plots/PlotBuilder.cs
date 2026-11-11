using System.Collections.Generic;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>Builds an owned plot: its floor and its content (see <see cref="PlotKind"/>).</summary>
    public static class PlotBuilder
    {
        /// <summary>Workers wait here until the NavMesh is rebaked (agents must spawn on it).</summary>
        private static readonly List<(WorkerType type, Vector3 pos)> PendingHires = new();

        public static Transform Build(Transform mapRoot, PlotSave plot, Customers.CustomerSpawner spawner)
        {
            Vector2Int cell = plot.Cell;
            var root = new GameObject($"Plot_{cell.x}_{cell.y}_{plot.Kind}").transform;
            root.SetParent(mapRoot, false);
            PlotVisuals.Floor(root, cell, SurfaceFor(plot.Kind));

            switch (plot.Kind)
            {
                case PlotKind.Ovens:
                case PlotKind.Kitchen:
                    KitchenBuilder.Station(root, PlotGrid.Local(cell, -1.8f, 1.6f), plot.param);
                    KitchenBuilder.Station(root, PlotGrid.Local(cell, 1.8f, 1.6f), plot.param);
                    break;
                case PlotKind.Counter:
                    KitchenBuilder.Counter(root, PlotGrid.Local(cell, -2.3f, 0.6f), spawner); // clear of the door walkway
                    break;
                case PlotKind.Dining: DecorBuilder.Dining(root, cell); break;
                case PlotKind.Storage: DecorBuilder.Storage(root, cell); break;
                case PlotKind.Garden: DecorBuilder.Garden(root, cell); break;
                case PlotKind.Parking: DecorBuilder.Parking(root, cell); break;
                case PlotKind.DriveThru: DecorBuilder.DriveThru(root, cell); break;
                case PlotKind.StaffRoom:
                    DecorBuilder.StaffRoom(root, cell);
                    PendingHires.Add(((WorkerType)plot.param, PlotGrid.Local(cell, 0f, 0.6f)));
                    break;
            }
            return root;
        }

        /// <summary>Spawn queued workers; call after the NavMesh covers their plots.</summary>
        public static void FlushHires()
        {
            foreach (var (type, pos) in PendingHires) WorkerHiring.Hire(type, pos);
            PendingHires.Clear();
        }

        private static Core.FloorTheme.Surface SurfaceFor(PlotKind kind) => kind switch
        {
            PlotKind.Ovens or PlotKind.Kitchen => Core.FloorTheme.Surface.Tiles,
            PlotKind.Garden => Core.FloorTheme.Surface.Grass,
            PlotKind.Parking or PlotKind.DriveThru => Core.FloorTheme.Surface.Asphalt,
            _ => Core.FloorTheme.Surface.Wood
        };
    }
}
