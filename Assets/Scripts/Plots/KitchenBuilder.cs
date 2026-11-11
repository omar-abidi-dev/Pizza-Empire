using UnityEngine;
using UnityEngine.AI;

namespace ShopIdle.Plots
{
    /// <summary>Runtime cooking stations and serving counters for plots.</summary>
    public static class KitchenBuilder
    {
        private static readonly Color OvenMarker = new(1f, 0.55f, 0f, 0.55f);
        private static readonly Color CashierMarker = new(0.25f, 0.78f, 0.75f, 0.5f);
        private static readonly Color MoneyMarker = new(0.35f, 0.82f, 0.4f, 0.5f);

        /// <summary>
        /// Cooking station for <paramref name="foodType"/> (oven / grill / sushi bar / taco cart),
        /// front facing -Z with its pickup marker in front.
        /// </summary>
        public static Kitchen.Oven Station(Transform parent, Vector3 pos, int foodType)
        {
            var root = new GameObject($"Station_{Kitchen.FoodCatalog.Name(foodType)}");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(pos.x, 0.5f, pos.z);

            var body = root.AddComponent<BoxCollider>();
            body.center = new Vector3(0f, 0.1f, 0f);
            body.size = new Vector3(1.5f, 1.2f, 1.2f);
            var obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.size = body.size;
            obstacle.center = body.center;
            obstacle.carving = true;

            var tray = new GameObject("TrayAnchor");
            tray.transform.SetParent(root.transform, false);

            var trigger = new GameObject("PickupTrigger");
            trigger.transform.SetParent(root.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var col = trigger.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(3f, 3f, 3f);

            var oven = trigger.AddComponent<Kitchen.Oven>();
            oven.SetFoodType(foodType);
            oven.SetTrayAnchor(tray.transform);

            // Visual after the Oven + tray exist so the tray aligns to the station top.
            Core.VisualFactory.BuildOvenVisual(root.transform);
            PlotVisuals.Marker(root.transform, pos + new Vector3(0f, 0f, -1.2f), 2.5f, 2f, OvenMarker);
            return oven;
        }

        /// <summary>
        /// Counter with a cashier spot behind it, a money spot on its top and a short queue in front
        /// (same layout as the starting counter).
        /// </summary>
        public static void Counter(Transform parent, Vector3 c, Customers.CustomerSpawner spawner)
        {
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "CounterModelRoot";
            counter.transform.SetParent(parent, false);
            counter.transform.position = new Vector3(c.x, 0.5f, c.z);
            counter.transform.localScale = new Vector3(3f, 1f, 0.8f);
            counter.GetComponent<MeshRenderer>().enabled = false;
            counter.AddComponent<NavMeshObstacle>().carving = true;
            Core.VisualFactory.BuildCounterVisual(counter.transform);

            var spot = Trigger(parent, "MoneySpot", c + new Vector3(1f, 0.5f, 0.5f), new Vector3(2f, 2f, 2.5f));
            var anchor = new GameObject("BillAnchor").transform;
            anchor.SetParent(spot.transform, false);
            anchor.position = c + new Vector3(1f, 0.84f, 0f);
            var money = spot.AddComponent<Money.MoneySpot>();
            money.SetBillAnchor(anchor);

            var queueObj = new GameObject("CounterQueue");
            queueObj.transform.SetParent(parent, false);
            var start = new GameObject("QueueStart").transform;
            start.SetParent(queueObj.transform, false);
            start.position = c + new Vector3(0f, 0f, -2f);
            var queue = queueObj.AddComponent<Customers.CustomerQueue>();
            queue.SetQueueStart(start);
            queue.SetMaxSize(3);

            var cashier = Trigger(parent, "CashierZone", c + new Vector3(0f, 0.5f, 1f), new Vector3(2.5f, 2f, 2f));
            cashier.AddComponent<Customers.CounterZone>().Init(queue, spawner, money);

            PlotVisuals.Marker(parent, c + new Vector3(0f, 0f, 1f), 2.5f, 2f, CashierMarker);
            PlotVisuals.Marker(parent, c + new Vector3(1f, 0f, 0f), 1.2f, 1f, MoneyMarker);
        }

        private static GameObject Trigger(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            return go;
        }
    }
}
