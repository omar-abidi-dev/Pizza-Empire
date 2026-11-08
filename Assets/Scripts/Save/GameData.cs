using System;

namespace ShopIdle.Save
{
    [Serializable]
    public class GameData
    {
        public int money;
        public int purchasedUpgradeCount;
        public int activeZonePartialSpent;
        public float playerX;
        public float playerY;
        public float playerZ;
        public long lastSaveTimestamp;
    }
}
