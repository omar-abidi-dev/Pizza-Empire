using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// Geometry of the endless plot map: 8x8 m cells, plot (0,0) is the starting kitchen
    /// (x -4..4, z 2..10). Grid +Y is world +Z.
    /// </summary>
    public static class PlotGrid
    {
        public const float Size = 8f;
        public const float Half = Size * 0.5f;
        private static readonly Vector3 Origin = new(0f, 0f, 6f);

        /// <summary>South, west, north, east (south/west face the camera).</summary>
        public static readonly Vector2Int[] Dirs =
        {
            new(0, -1), new(-1, 0), new(0, 1), new(1, 0)
        };

        public static Vector3 Center(Vector2Int c) => Origin + new Vector3(c.x * Size, 0f, c.y * Size);

        /// <summary>World position of a plot-local offset (metres from the plot centre).</summary>
        public static Vector3 Local(Vector2Int c, float x, float z) => Center(c) + new Vector3(x, 0f, z);

        public static Vector2Int CellOf(Vector3 world)
        {
            Vector3 d = world - Origin;
            return new Vector2Int(Mathf.RoundToInt(d.x / Size), Mathf.RoundToInt(d.z / Size));
        }

        /// <summary>
        /// Where the buy pad for <paramref name="locked"/> sits: just inside <paramref name="owned"/>
        /// against their shared wall, off-centre so it clears doors and queues. Plot layouts keep
        /// these spots free (x 1.2..3.6 by north/south walls, z -3.6..-1.2 by east/west walls).
        /// </summary>
        public static Vector3 TileSpot(Vector2Int owned, Vector2Int locked)
        {
            Vector3 mid = (Center(owned) + Center(locked)) * 0.5f;
            bool horizontalEdge = owned.x == locked.x;
            Vector3 inward = (Center(owned) - Center(locked)).normalized * 1.15f; // whole pad inside the owned plot
            return mid + inward + (horizontalEdge ? new Vector3(2.4f, 0f, 0f) : new Vector3(0f, 0f, -2.4f));
        }
    }

    /// <summary>What a bought plot turns into.</summary>
    public enum PlotKind
    {
        Start = 0,
        Ovens = 1,
        Dining = 2,
        DriveThru = 3,
        Storage = 4,
        Counter = 5,
        Garden = 6,
        StaffRoom = 7,
        Parking = 8,
        Kitchen = 9   // milestone plot: stations for a newly added food
    }

    /// <summary>One owned plot in the save file.</summary>
    [System.Serializable]
    public class PlotSave
    {
        public int x;
        public int y;
        public int kind;
        public int param;   // food type (Ovens/Kitchen) or worker type (StaffRoom)

        public Vector2Int Cell => new(x, y);
        public PlotKind Kind => (PlotKind)kind;
    }
}
