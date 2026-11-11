using UnityEngine;

namespace ShopIdle.Money
{
    /// <summary>
    /// Layout for cash piles: bill stacks fill a 3x3 grid, then the next layer starts on top,
    /// so a pile grows upward as money accumulates.
    /// </summary>
    public static class CashPile
    {
        /// <summary>Bill stacks are turned 90° so their long side runs along world Z.</summary>
        public const float StackYaw = 90f;
        public const float YawJitter = 3f;

        private const int GridSize = 3;
        private const int PerLayer = GridSize * GridSize;

        /// <summary>World-space offset of stack <paramref name="index"/> from the pile centre.</summary>
        public static Vector3 Offset(int index, float cellX, float cellZ, float layerHeight)
        {
            int cell = index % PerLayer;
            int layer = index / PerLayer;
            int col = cell % GridSize - 1;
            int row = cell / GridSize - 1;
            return new Vector3(col * cellX, layer * layerHeight, row * cellZ);
        }

        public static float RandomYaw() => StackYaw + Random.Range(-YawJitter, YawJitter);
    }
}
