using UnityEngine;

namespace ShopIdle.Core
{
    /// <summary>
    /// Applies the "Sunny Toy Shop" visual overhaul to the scene on startup.
    /// Rebuilds player, environment, and existing prop visuals.
    /// </summary>
    [DefaultExecutionOrder(50)] // Run after other managers
    public class SceneThemer : MonoBehaviour
    {
        private static SceneThemer _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            ApplyAll();
        }

        private void ApplyAll()
        {
            // Ensure camera has CameraFollow
            EnsureCameraSetup();

            // Environment: floor checker, wall coral + trim, lighting
            VisualFactory.ApplyEnvironmentTheme();

            // Player visual overhaul
            ApplyPlayerVisual();

            // Starting oven overhaul (plot stations build their own visuals)
            ApplyOvenVisuals();

            // Counter overhaul
            ApplyCounterVisuals();

            // Tables
            ApplyTableVisuals();

            // Decorative props (trash bin)
            PropDecor.PlaceAll();

            // Grass under the whole (endless) plot map
            CreateOutdoorGrass();
        }

        private void EnsureCameraSetup()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var follow = cam.GetComponent<CameraSystem.CameraFollow>();
            if (follow == null)
                cam.gameObject.AddComponent<CameraSystem.CameraFollow>();
        }

        private void ApplyPlayerVisual()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            // Remove old MeshRenderer/MeshFilter if the player is a primitive capsule
            var oldMr = player.GetComponent<MeshRenderer>();
            var oldMf = player.GetComponent<MeshFilter>();
            if (oldMr != null) oldMr.enabled = false;

            VisualFactory.BuildPlayerVisual(player.transform);
        }

        private void ApplyOvenVisuals()
        {
            // Find ovens by name rather than component (component may be on a child)
            string[] ovenNames = { "Oven" };
            foreach (string name in ovenNames)
            {
                var ovenObj = GameObject.Find(name);
                if (ovenObj == null) continue;

                var mr = ovenObj.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;

                VisualFactory.BuildOvenVisual(ovenObj.transform);
            }
        }

        private void ApplyCounterVisuals()
        {
            string[] counterNames = { "Counter" };
            foreach (string name in counterNames)
            {
                var counterObj = GameObject.Find(name);
                if (counterObj == null) continue;

                var mr = counterObj.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;

                VisualFactory.BuildCounterVisual(counterObj.transform);
            }
        }

        private void ApplyTableVisuals()
        {
            var tables = Object.FindObjectsByType<Tables.DineInTable>(FindObjectsSortMode.None);
            foreach (var table in tables)
                VisualFactory.BuildTableVisual(table.transform);
        }

        private void CreateOutdoorGrass()
        {
            if (GameObject.Find("OutdoorGrass") != null) return;

            // Large grass plane behind/around the shop
            var grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
            grass.name = "OutdoorGrass";
            grass.transform.position = new Vector3(0f, -0.05f, 0f);
            grass.transform.localScale = new Vector3(40f, 1f, 40f); // 400 m: room for many plots
            Object.Destroy(grass.GetComponent<MeshCollider>());
            FloorTheme.TextureGrass(grass);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
