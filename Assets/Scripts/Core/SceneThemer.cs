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

            // Existing oven overhaul
            ApplyOvenVisuals();

            // Counter overhaul
            ApplyCounterVisuals();

            // Tables
            ApplyTableVisuals();

            // Decorative props (trash bin)
            PropDecor.PlaceAll();

            // Add grass outside
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
            string[] ovenNames = { "Oven", "Oven2", "Oven3" };
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
            string[] counterNames = { "Counter", "Counter2" };
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

            var p = ColorPalette.Instance;

            // Large grass plane behind/around the shop
            var grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
            grass.name = "OutdoorGrass";
            grass.transform.position = new Vector3(0f, -0.05f, 0f);
            grass.transform.localScale = new Vector3(8f, 1f, 8f);
            Object.Destroy(grass.GetComponent<MeshCollider>());
            FloorTheme.TextureGrass(grass);
        }

        /// <summary>Called when expansion unlocks to retheme new areas.</summary>
        public void OnExpansionUnlocked()
        {
            // Zoom out camera
            var cam = Object.FindAnyObjectByType<CameraSystem.CameraFollow>();
            if (cam != null) cam.ZoomOut();

            // Kitchen tiles continue into the expansion (third oven / drive-thru side)
            FloorTheme.AddKitchenTiles("KitchenTilesExpansion", 8.15f, 19.85f);

            // Restyle expansion walls
            var p = ColorPalette.Instance;
            foreach (string wallName in new[] { "ExpWallRight_Upper", "ExpWallRight_Lower", "ExpWallBack" })
            {
                var wall = GameObject.Find(wallName);
                if (wall == null) continue;
                var mr = wall.GetComponent<MeshRenderer>();
                if (mr != null) mr.material = ColorPalette.MakeLit(p.wallCoral);

                // Add trim
                string trimName = wallName + "_Trim";
                if (wall.transform.Find(trimName) == null)
                {
                    var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    trim.name = trimName;
                    trim.transform.SetParent(wall.transform);
                    trim.transform.localPosition = new Vector3(0f, 0.48f, 0f);
                    trim.transform.localScale = new Vector3(1.02f, 0.04f, 1.02f);
                    Object.Destroy(trim.GetComponent<BoxCollider>());
                    trim.GetComponent<MeshRenderer>().material = ColorPalette.MakeLit(p.wallTrim);
                }
            }

            // Re-theme road
            var road = GameObject.Find("Road");
            if (road != null)
            {
                var mr = road.GetComponent<MeshRenderer>();
                if (mr != null) mr.material = ColorPalette.MakeLit(p.roadGrey);
            }
            var roadLine = GameObject.Find("RoadLine");
            if (roadLine != null)
            {
                var mr = roadLine.GetComponent<MeshRenderer>();
                if (mr != null) mr.material = ColorPalette.MakeLit(p.roadDash);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
