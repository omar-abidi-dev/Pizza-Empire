using UnityEngine;
using UnityEngine.AI;

namespace ShopIdle.Core
{
    /// <summary>
    /// Places purely decorative prop models (no gameplay role): the trash bin and plants in the
    /// starting plot, and plants for dining plots. Spots avoid buy pads, queues and stations.
    /// </summary>
    public static class PropDecor
    {
        // Starting plot (x -4..4, z 2..10): back-left corner behind the oven.
        private static readonly Vector3 TrashBinPos = new(-3.45f, 0f, 9.45f);
        private const float TrashBinYaw = 135f; // flap faces into the room

        private static readonly Vector3 StartPlantPos = new(3.4f, 0f, 6.1f);
        private static readonly Vector3 StartTallPlantPos = new(-1.7f, 0f, 9.5f);

        public static void PlaceAll()
        {
            var m = PropModels.Instance;
            if (m == null || GameObject.Find("TrashBin") != null) return;
            Solid(Place(m.TrashBin, null, TrashBinPos, TrashBinYaw, "TrashBin"), 0.3f, 0.9f);
            Plant(m.Plant, null, StartPlantPos);
            Plant(m.PlantTall, null, StartTallPlantPos);
        }

        /// <summary>A potted plant the player bumps into (no-op without a model).</summary>
        public static void Plant(GameObject model, Transform parent, Vector3 pos)
        {
            var plant = Place(model, parent, pos, Random.Range(0f, 360f), "Plant");
            if (plant != null) Solid(plant, 0.22f, 0.8f);
        }

        private static GameObject Place(GameObject model, Transform parent, Vector3 pos, float yaw, string name)
        {
            if (model == null) return null;
            var obj = PropSpawner.Spawn(model, parent, pos, yaw);
            obj.name = name;
            return obj;
        }

        /// <summary>Capsule collider + carving obstacle so the player and NPCs walk around it.</summary>
        private static void Solid(GameObject obj, float radius, float height)
        {
            if (obj == null) return;
            var col = obj.AddComponent<CapsuleCollider>();
            col.radius = radius;
            col.height = height;
            col.center = new Vector3(0f, height / 2f, 0f);

            var obstacle = obj.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = radius;
            obstacle.height = height;
            obstacle.center = col.center;
            obstacle.carving = true;
        }
    }
}
