using UnityEngine;

namespace ShopIdle.Kitchen
{
    /// <summary>
    /// The restaurant menu grows with the plot map: a new food type is added at each milestone
    /// (3, 6, 10, 15, 21… plots owned). Customers order a random mix of unlocked foods.
    /// </summary>
    public static class Menu
    {
        /// <summary>Food types currently on the menu (always at least 1).</summary>
        public static int Count { get; private set; } = 1;

        public static event System.Action<int> OnFoodAdded;

        /// <summary>Plots owned when the menu reaches <paramref name="size"/> items (size ≥ 2).</summary>
        public static int MilestoneFor(int size) => (size + 1) * (size + 2) / 2 - 3;

        /// <summary>Menu size implied by a number of owned plots.</summary>
        public static int SizeFor(int plotsOwned)
        {
            int size = 1;
            while (MilestoneFor(size + 1) <= plotsOwned) size++;
            return size;
        }

        /// <summary>Set menu size; raises <see cref="OnFoodAdded"/> for each new item when <paramref name="announce"/>.</summary>
        public static void SetSize(int size, bool announce)
        {
            size = Mathf.Max(1, size);
            int old = Count;
            Count = size;
            if (!announce) return;
            for (int t = old; t < size; t++) OnFoodAdded?.Invoke(t);
        }

        public static int RandomType() => Random.Range(0, Count);
    }
}
