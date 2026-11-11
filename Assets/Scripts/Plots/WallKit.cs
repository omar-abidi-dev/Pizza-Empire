using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// Modelled wall details (walls_v2.py): corner posts, window frames and the entrance
    /// gateway. Each method returns false / null when its model is missing so PlotWalls can
    /// fall back to primitives.
    /// </summary>
    public static class WallKit
    {
        private const float WindowSill = 0.95f;

        private static Core.PropModels M => Core.PropModels.Instance;

        public static GameObject Post(Transform parent, Vector3 pos, bool tall)
        {
            var model = M != null ? (tall ? M.PostTall : M.PostLow) : null;
            if (model == null) return null;
            var post = Core.PropSpawner.Spawn(model, parent, pos);
            post.name = tall ? "PostTall" : "PostLow";
            return post;
        }

        /// <summary>Two window frames on the room-side face of a tall wall.</summary>
        public static bool Windows(Transform parent, Vector3 wallCenter, bool alongX, float thickness)
        {
            var model = M != null ? M.WindowFrame : null;
            if (model == null) return false;
            Vector3 axis = alongX ? Vector3.right : Vector3.forward;
            // Room side: -Z for north walls, -X for east walls. Model front (+Z at yaw 0) turns to face it.
            Vector3 inward = alongX ? Vector3.back : Vector3.left;
            float yaw = alongX ? 180f : -90f;
            foreach (float offset in new[] { -2f, 2f })
            {
                Vector3 pos = wallCenter + axis * offset + inward * (thickness * 0.5f) + Vector3.up * WindowSill;
                Core.PropSpawner.Spawn(model, parent, pos, yaw).name = "WindowFrame";
            }
            return true;
        }

        /// <summary>Awning gateway over the customer door, facing the street (-Z).</summary>
        public static void Entrance(Transform parent, Vector3 doorMid)
        {
            var model = M != null ? M.Entrance : null;
            if (model == null) return;
            Core.PropSpawner.Spawn(model, parent, doorMid, 180f).name = "Entrance";
        }
    }
}
