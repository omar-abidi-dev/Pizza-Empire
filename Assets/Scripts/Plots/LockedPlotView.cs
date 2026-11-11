using UnityEngine;

namespace ShopIdle.Plots
{
    /// <summary>
    /// A plot that is visible but not bought: faded planks with a dashed outline. Plots next to
    /// the owned area also get a price tile on their shared boundary.
    /// </summary>
    public class LockedPlotView : MonoBehaviour
    {
        private static Mesh _dashMesh;
        private static Material _dashMat;

        private Upgrades.UpgradeZone _zone;

        public Vector2Int Cell { get; private set; }
        public Upgrades.UpgradeZone Zone => _zone;

        public static LockedPlotView Create(Transform parent, Vector2Int cell)
        {
            var go = new GameObject($"LockedPlot_{cell.x}_{cell.y}");
            go.transform.SetParent(parent, false);
            go.transform.position = PlotGrid.Center(cell);
            var view = go.AddComponent<LockedPlotView>();
            view.Cell = cell;
            view.BuildVisual();
            return view;
        }

        private void BuildVisual()
        {
            PlotVisuals.Quad(transform, "Faded", new Vector3(0f, 0.003f, 0f),
                PlotGrid.Size - 0.2f, PlotGrid.Size - 0.2f, Core.FloorTheme.LockedMaterial());

            _dashMesh ??= PlotVisuals.DashedSquare(PlotGrid.Size - 0.3f, 0.55f, 0.35f, 0.09f);
            _dashMat ??= Core.ColorPalette.MakeTransparent(new Color(1f, 1f, 1f, 0.85f));
            var outline = new GameObject("DashedOutline");
            outline.transform.SetParent(transform, false);
            outline.transform.localPosition = new Vector3(0f, 0.008f, 0f);
            outline.AddComponent<MeshFilter>().sharedMesh = _dashMesh;
            outline.AddComponent<MeshRenderer>().sharedMaterial = _dashMat;
        }

        /// <summary>Show (or reprice) the price tile on the boundary with <paramref name="ownedNeighbour"/>.</summary>
        public void ShowPriceTile(Vector2Int ownedNeighbour, int price, System.Action<LockedPlotView> onBought)
        {
            if (_zone != null)
            {
                _zone.Reprice(price);
                return;
            }
            var zoneObj = new GameObject("PriceTile");
            zoneObj.transform.SetParent(transform, false);
            zoneObj.transform.position = PlotGrid.TileSpot(ownedNeighbour, Cell);
            _zone = zoneObj.AddComponent<Upgrades.UpgradeZone>();
            _zone.Init("New Plot", price);
            _zone.OnPurchased += () => onBought(this);
        }
    }
}
