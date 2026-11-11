using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// The endless plot map. Owns the set of bought plots, shows locked neighbours with price
    /// tiles, builds random content for each purchase, rebakes the NavMesh and grows the menu.
    /// Price of the next plot = BasePrice * 1.35^plotsOwned.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    [DefaultExecutionOrder(20)] // after PlayerController restores the saved position
    public class PlotManager : MonoBehaviour
    {
        [SerializeField] private int _basePrice = 100;
        [SerializeField] private float _priceGrowth = 1.35f;

        private readonly HashSet<Vector2Int> _owned = new();
        private readonly Dictionary<Vector2Int, LockedPlotView> _locked = new();
        private List<PlotSave> _plots;
        private PlotWalls _walls;
        private NavMeshSurface _surface;
        private Customers.CustomerSpawner _spawner;
        private UI.ProgressUI _progress;

        public int PlotsOwned => _owned.Count;
        public int NextPrice => Mathf.RoundToInt(_basePrice * Mathf.Pow(_priceGrowth, _owned.Count));

        private void Start()
        {
            _surface = GetComponent<NavMeshSurface>();
            _walls = new GameObject("PlotWalls").AddComponent<PlotWalls>();
            _walls.transform.SetParent(transform, false);
            _spawner = FindAnyObjectByType<Customers.CustomerSpawner>();

            var sm = Save.SaveManager.Instance;
            _plots = sm != null ? sm.Data.plots : new List<PlotSave>();
            if (_plots.Count == 0) _plots.Add(new PlotSave { kind = (int)PlotKind.Start });

            foreach (var plot in _plots)
            {
                _owned.Add(plot.Cell);
                PlotBuilder.Build(transform, plot, _spawner);
            }
            Kitchen.Menu.SetSize(Kitchen.Menu.SizeFor(_owned.Count), false);
            Kitchen.Menu.OnFoodAdded += OnFoodAdded;

            _progress = new GameObject("ProgressUI").AddComponent<UI.ProgressUI>();
            _progress.Init(Core.RestaurantManager.Instance != null
                ? Core.RestaurantManager.Instance.CurrentConfig.restaurantName : "Pizza Palace");

            Upgrades.GuideArrow.Create();

            Refresh(false);
            _surface.BuildNavMesh();
            PlotBuilder.FlushHires();
            KeepPlayerInside();
        }

        private void OnDestroy() { Kitchen.Menu.OnFoodAdded -= OnFoodAdded; }

        private void Buy(LockedPlotView view)
        {
            Vector2Int cell = view.Cell;
            bool milestone = Kitchen.Menu.SizeFor(_owned.Count + 1) > Kitchen.Menu.Count;
            var plot = milestone
                ? new PlotSave { x = cell.x, y = cell.y, kind = (int)PlotKind.Kitchen, param = Kitchen.Menu.Count }
                : PlotContent.Roll(cell, _plots);

            _plots.Add(plot);
            _owned.Add(cell);
            _locked.Remove(cell);
            Destroy(view.gameObject);
            PlotBuilder.Build(transform, plot, _spawner);
            Refresh(true);

            Core.JuiceManager.Instance?.ShakeCamera(0.4f);
            if (milestone) Kitchen.Menu.SetSize(Kitchen.Menu.SizeFor(_owned.Count), true);
            else UI.Banner.Show($"{PlotContent.DisplayName(plot.Kind)}!");

            StartCoroutine(RebakeAfterWalls());
            Save.SaveManager.Instance?.SaveGame();
        }

        private IEnumerator RebakeAfterWalls()
        {
            yield return new WaitForSeconds(0.6f); // let the absorbed walls sink first
            _surface.BuildNavMesh();
            PlotBuilder.FlushHires();
        }

        private void OnFoodAdded(int type)
        {
            UI.Banner.Show($"NEW ON THE MENU: {Kitchen.FoodCatalog.Name(type).ToUpper()}!");
        }

        /// <summary>Walls, locked neighbours, price tiles, the customer door and plot effects.</summary>
        private void Refresh(bool animate)
        {
            Vector2Int door = DoorPlot();
            _walls.Rebuild(_owned, door, animate);

            int price = NextPrice;
            foreach (var cell in _owned)
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var n = cell + new Vector2Int(dx, dy);
                        if (_owned.Contains(n)) continue;
                        if (!_locked.TryGetValue(n, out var view))
                            _locked[n] = view = LockedPlotView.Create(transform, n);
                        if (dx == 0 || dy == 0) view.ShowPriceTile(cell, price, Buy);
                    }

            float doorZ = PlotGrid.Center(door).z - PlotGrid.Half;
            _spawner?.SetDoor(new Vector3(-0.6f, 0f, doorZ - 4.5f), new Vector3(0.6f, 0f, doorZ - 4.5f));
            Core.FloorTheme.PlaceFrontRoad(doorZ - 7.5f);
            PlotEffects.Apply(_plots, _owned);
            _progress?.Show(_owned.Count);
        }

        /// <summary>Customers enter through the southernmost owned plot of the starting column.</summary>
        private Vector2Int DoorPlot()
        {
            var door = Vector2Int.zero;
            while (_owned.Contains(door + Vector2Int.down)) door += Vector2Int.down;
            return door;
        }

        private void KeepPlayerInside()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            Vector3 pos = player.transform.position;
            if (pos.y > -1f && _owned.Contains(PlotGrid.CellOf(pos))) return;
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = new Vector3(2f, 0.5f, 4f);
            if (cc != null) cc.enabled = true;
        }
    }
}
