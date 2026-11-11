namespace ShopIdle.Core
{
    /// <summary>Global gameplay signals for progression UI (levels, objectives).</summary>
    public static class GameEvents
    {
        public static event System.Action CustomerServed;
        public static event System.Action PlotBought;
        public static event System.Action<int> CashCollected;

        public static void RaiseCustomerServed() => CustomerServed?.Invoke();
        public static void RaisePlotBought() => PlotBought?.Invoke();
        public static void RaiseCashCollected(int amount) => CashCollected?.Invoke(amount);
    }
}
