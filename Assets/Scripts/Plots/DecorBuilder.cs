using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>Furnishing for non-kitchen plots: dining, packing, garden, staff room, parking, drive-thru.</summary>
    public static class DecorBuilder
    {
        // Positions keep clear of the price-tile spots on the plot edges (see PlotGrid.TileSpot).
        private static readonly Vector2[] TableSpots = { new(-1.8f, 1.5f), new(1.8f, 1.5f), new(-1.8f, -1.2f), new(1.8f, -1.2f) };

        public static void Dining(Transform parent, Vector2Int cell)
        {
            if (Tables.TableManager.Instance == null) new GameObject("TableManager").AddComponent<Tables.TableManager>();
            foreach (var s in TableSpots)
            {
                var table = new GameObject("DineTable");
                table.transform.SetParent(parent, false);
                table.transform.position = PlotGrid.Local(cell, s.x, s.y);
                Core.VisualFactory.BuildTableVisual(table.transform);
                var col = table.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(3f, 2f, 2f);
                col.center = new Vector3(0f, 1f, 0f);
                Tables.TableManager.Instance.RegisterTable(table.AddComponent<Tables.DineInTable>());
            }
        }

        /// <summary>Shelves of boxes and a packing table (raises how much the player can carry).</summary>
        public static void Storage(Transform parent, Vector2Int cell)
        {
            var p = Core.ColorPalette.Instance;
            Color shelf = p.metalLight, box = new(0.85f, 0.66f, 0.42f), tape = new(0.95f, 0.85f, 0.6f);
            for (int i = 0; i < 2; i++)
            {
                float x = -2.2f + i * 2.4f;
                PlotVisuals.Solid(parent, PrimitiveType.Cube, PlotGrid.Local(cell, x, 2.2f) + Vector3.up * 0.9f,
                    new Vector3(2f, 1.8f, 0.6f), shelf);
                for (int row = 0; row < 3; row++)
                    for (int b = 0; b < 3; b++)
                        PlotVisuals.Prop(parent, PrimitiveType.Cube,
                            PlotGrid.Local(cell, x - 0.6f + b * 0.6f, 1.85f) + Vector3.up * (0.3f + row * 0.55f),
                            new Vector3(0.45f, 0.4f, 0.4f), (row + b) % 2 == 0 ? box : tape);
            }
            PlotVisuals.Solid(parent, PrimitiveType.Cube, PlotGrid.Local(cell, -0.5f, -0.6f) + Vector3.up * 0.45f,
                new Vector3(2.2f, 0.9f, 1f), p.tableWood);
            PlotVisuals.Prop(parent, PrimitiveType.Cube, PlotGrid.Local(cell, -0.9f, -0.6f) + Vector3.up * 1.1f,
                new Vector3(0.5f, 0.4f, 0.5f), box);
        }

        public static void Garden(Transform parent, Vector2Int cell)
        {
            Vector2[] trees = { new(-2.3f, 2f), new(2.2f, 0.4f), new(-1.6f, -1.8f) };
            foreach (var t in trees)
            {
                Vector3 pos = PlotGrid.Local(cell, t.x, t.y);
                PlotVisuals.Solid(parent, PrimitiveType.Cylinder, pos + Vector3.up * 0.6f, new Vector3(0.3f, 0.6f, 0.3f),
                    new Color(0.55f, 0.38f, 0.25f));
                PlotVisuals.Prop(parent, PrimitiveType.Sphere, pos + Vector3.up * 1.7f, Vector3.one * 1.5f,
                    new Color(0.42f, 0.75f, 0.42f));
            }
            Color[] flowers = { new(1f, 0.55f, 0.65f), new(1f, 0.85f, 0.35f), new(0.75f, 0.6f, 1f) };
            for (int i = 0; i < 9; i++)
            {
                float a = i * 0.7f;
                Vector3 pos = PlotGrid.Local(cell, Mathf.Cos(a) * (0.6f + i * 0.2f), Mathf.Sin(a) * (0.6f + i * 0.2f));
                PlotVisuals.Prop(parent, PrimitiveType.Sphere, pos + Vector3.up * 0.2f, Vector3.one * 0.45f, flowers[i % 3]);
            }
            PlotVisuals.Solid(parent, PrimitiveType.Cube, PlotGrid.Local(cell, 0.6f, 2.4f) + Vector3.up * 0.25f,
                new Vector3(1.6f, 0.5f, 0.5f), Core.ColorPalette.Instance.tableWood);
        }

        public static void StaffRoom(Transform parent, Vector2Int cell)
        {
            Color[] lockers = { new(0.45f, 0.65f, 0.9f), new(0.95f, 0.6f, 0.5f), new(0.55f, 0.8f, 0.6f), new(0.95f, 0.8f, 0.45f) };
            for (int i = 0; i < 4; i++)
                PlotVisuals.Solid(parent, PrimitiveType.Cube, PlotGrid.Local(cell, -2.4f + i * 0.75f, 2.5f) + Vector3.up * 0.9f,
                    new Vector3(0.7f, 1.8f, 0.6f), lockers[i]);
            var p = Core.ColorPalette.Instance;
            PlotVisuals.Solid(parent, PrimitiveType.Cylinder, PlotGrid.Local(cell, 1.2f, -0.6f) + Vector3.up * 0.4f,
                new Vector3(1.2f, 0.4f, 1.2f), p.tableWood);
            PlotVisuals.Prop(parent, PrimitiveType.Cylinder, PlotGrid.Local(cell, 1.2f, -0.6f) + Vector3.up * 0.9f,
                new Vector3(0.18f, 0.1f, 0.18f), Color.white);
            PlotVisuals.Solid(parent, PrimitiveType.Cube, PlotGrid.Local(cell, -1.6f, -0.6f) + Vector3.up * 0.25f,
                new Vector3(0.6f, 0.5f, 1.8f), p.chairSeat);
        }

        public static void Parking(Transform parent, Vector2Int cell)
        {
            Color[] cars = { new(0.92f, 0.55f, 0.55f), new(0.55f, 0.65f, 0.92f), new(0.65f, 0.88f, 0.65f) };
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = PlotGrid.Local(cell, -2.4f + i * 2.4f, 1.4f);
                PlotVisuals.Solid(parent, PrimitiveType.Cube, pos + Vector3.up * 0.4f, new Vector3(1.4f, 0.6f, 2.4f), cars[i]);
                PlotVisuals.Prop(parent, PrimitiveType.Cube, pos + new Vector3(0f, 0.85f, -0.2f), new Vector3(1.2f, 0.45f, 1.2f), cars[i]);
            }
        }

        /// <summary>Booth, window and lane: cars queue along +X of the window, entering from +Z.</summary>
        public static void DriveThru(Transform parent, Vector2Int cell)
        {
            var lane = PlotVisuals.Quad(parent, "Lane", Vector3.zero, PlotGrid.Size, 3f, Core.FloorTheme.LaneMaterial());
            lane.transform.SetPositionAndRotation(PlotGrid.Local(cell, 2.5f, 0f) + Vector3.up * 0.012f,
                Quaternion.Euler(90f, 90f, 0f));

            Vector3 window = PlotGrid.Local(cell, 0.5f, -1.5f);
            if (!Core.VisualFactory.BuildDriveThruBooth(window + Vector3.right))
                PlotVisuals.Prop(parent, PrimitiveType.Cube, window + new Vector3(0.7f, 0.7f, 0f),
                    new Vector3(0.6f, 0.08f, 1.5f), new Color(0.6f, 0.4f, 0.2f));

            var windowObj = new GameObject("DriveThruWindow");
            windowObj.transform.SetParent(parent, false);
            windowObj.transform.position = window;
            windowObj.AddComponent<DriveThru.DriveThruWindow>();
            PlotVisuals.Marker(parent, window, 2.5f, 2.5f, new Color(0.2f, 0.8f, 1f, 0.5f));
        }
    }
}
