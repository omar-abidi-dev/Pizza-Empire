using UnityEngine;
using UnityEngine.AI;

namespace ShopIdle.Core
{
    /// <summary>
    /// Places purely decorative prop models (no gameplay role) in the restaurant.
    /// </summary>
    public static class PropDecor
    {
        // Front-right corner: away from the customer spawn, queue and dining tables.
        private static readonly Vector3 TrashBinPos = new(7.2f, 0f, -9.2f);
        private const float TrashBinYaw = -45f; // flap faces into the room

        public static void PlaceAll()
        {
            PlaceTrashBin();
        }

        private static void PlaceTrashBin()
        {
            var model = PropModels.Instance != null ? PropModels.Instance.TrashBin : null;
            if (model == null || GameObject.Find("TrashBin") != null) return;

            var bin = PropSpawner.Spawn(model, null, TrashBinPos, TrashBinYaw);
            bin.name = "TrashBin";

            // Solid so the player and NPCs walk around it.
            var col = bin.AddComponent<CapsuleCollider>();
            col.radius = 0.3f;
            col.height = 0.9f;
            col.center = new Vector3(0f, 0.45f, 0f);

            var obstacle = bin.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = 0.3f;
            obstacle.height = 0.9f;
            obstacle.center = new Vector3(0f, 0.45f, 0f);
            obstacle.carving = true;
        }
    }
}
