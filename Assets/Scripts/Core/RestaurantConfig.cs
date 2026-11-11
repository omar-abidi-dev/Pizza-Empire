using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Data-driven restaurant definition. Created at runtime via ScriptableObject.CreateInstance.
    /// Each restaurant has unique theming, food visuals, and economy scaling.
    /// </summary>
    public class RestaurantConfig : ScriptableObject
    {
        public string restaurantName;
        public string foodName;

        [Header("Food Appearance")]
        public Color foodColor;
        public PrimitiveType foodShape;
        public Vector3 foodScale;

        [Header("Environment")]
        public Color floorColor;
        public Color wallColor;
        public Color ovenColor;

        [Header("Economy")]
        public int moneyPerItem;
        public int driveThruMoneyPerItem;
        public int startingBonus;
    }
}
