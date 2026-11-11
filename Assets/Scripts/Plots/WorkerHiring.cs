using UnityEngine;
using UnityEngine.AI;

namespace ShopIdle.Plots
{
    public enum WorkerType { Chef = 0, Cashier = 1, Cleaner = 2, DriveThru = 3 }

    /// <summary>Creates NPC workers for staff rooms (needs a baked NavMesh for walkers).</summary>
    public static class WorkerHiring
    {
        /// <summary>The most useful worker to hire next, judged from what the restaurant has.</summary>
        public static WorkerType NextHire()
        {
            int chefs = Object.FindObjectsByType<Workers.ChefWorker>(FindObjectsSortMode.None).Length;
            if (chefs == 0) return WorkerType.Chef;
            if (FreeCounter() != null) return WorkerType.Cashier;

            int cleaners = Object.FindObjectsByType<Workers.CleanerWorker>(FindObjectsSortMode.None).Length;
            int tables = Object.FindObjectsByType<Tables.DineInTable>(FindObjectsSortMode.None).Length;
            if (tables > cleaners * 8) return WorkerType.Cleaner;

            bool dtWorker = Object.FindAnyObjectByType<Workers.DriveThruWorker>() != null;
            if (DriveThru.DriveThruWindow.Instance != null && !dtWorker) return WorkerType.DriveThru;
            return WorkerType.Chef;
        }

        public static void Hire(WorkerType type, Vector3 position)
        {
            switch (type)
            {
                case WorkerType.Cashier: HireCashier(position); break;
                case WorkerType.Cleaner: SpawnWalker<Workers.CleanerWorker>("CleanerWorker", "Cleaner", position, 3f, 1.5f); break;
                case WorkerType.DriveThru: SpawnWalker<Workers.DriveThruWorker>("DriveThruWorker", "DriveThru", position, 3.5f, 1.8f); break;
                default: SpawnWalker<Workers.ChefWorker>("ChefWorker", "Chef", position, 3.5f, 1.8f); break;
            }
        }

        private static Customers.CounterZone FreeCounter()
        {
            foreach (var c in Customers.CounterZone.All)
                if (!c.HasCashier) return c;
            return null;
        }

        /// <summary>A cashier stands behind a counter without one; falls back to a chef.</summary>
        private static void HireCashier(Vector3 fallback)
        {
            var counter = FreeCounter();
            if (counter == null)
            {
                SpawnWalker<Workers.ChefWorker>("ChefWorker", "Chef", fallback, 3.5f, 1.8f);
                return;
            }
            counter.EnableCashierWorker();

            var worker = new GameObject("CashierWorker");
            worker.transform.SetPositionAndRotation(counter.transform.position + new Vector3(0f, 0f, 0.5f),
                Quaternion.Euler(0f, 180f, 0f));
            Core.VisualFactory.BuildWorkerVisual(worker.transform, "Cashier");
            worker.AddComponent<Workers.CashierWorker>();
        }

        private static void SpawnWalker<T>(string name, string visual, Vector3 position, float speed, float stop)
            where T : MonoBehaviour
        {
            var worker = new GameObject(name);
            if (NavMesh.SamplePosition(position, out var hit, 4f, NavMesh.AllAreas))
                position = hit.position;
            worker.transform.position = new Vector3(position.x, 0.5f, position.z);
            Core.VisualFactory.BuildWorkerVisual(worker.transform, visual);

            var agent = worker.AddComponent<NavMeshAgent>();
            agent.speed = speed;
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.stoppingDistance = stop;
            worker.AddComponent<T>();
        }
    }
}
