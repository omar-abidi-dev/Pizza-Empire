using System;
using System.Collections.Generic;

namespace ShopIdle.Save
{
    [Serializable]
    public class GameData
    {
        public int money;
        public int purchasedUpgradeCount;   // = plots owned (kept for older saves)

        // Endless plot map: every bought plot in purchase order
        public List<Plots.PlotSave> plots = new();
        public int activeZonePartialSpent;
        public float playerX;
        public float playerY;
        public float playerZ;
        public long lastSaveTimestamp;

        // Multi-restaurant
        public int restaurantIndex;
        public int restaurantsCompleted;
        public long totalMoneyEarned;

        // Prestige
        public int prestigeLevel;

        // Gems
        public int gems;

        // Level / XP and the current objective
        public int level = 1;
        public int xp;
        public int objectiveIndex;
        public int objectiveProgress;
        public bool tutorialDone;

        // Daily quests
        public string dailyQuestDate;          // "yyyy-MM-dd"
        public List<DailyQuestData> dailyQuests = new();

        // Stats for quest tracking
        public int todayCustomersServed;
        public int todayMoneyCollected;
        public int todayTablesCleaned;
    }

    [Serializable]
    public class DailyQuestData
    {
        public int questType;  // maps to DailyQuestType enum
        public int target;
        public int progress;
        public int gemReward;
        public bool claimed;
    }
}
