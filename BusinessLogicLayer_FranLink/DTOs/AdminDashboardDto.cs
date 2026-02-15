namespace BusinessLogicLayer_FranLink.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalStores { get; set; }
        public int TotalCentralKitchens { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }

        public Dictionary<string, int> OrdersByStatus { get; set; } = new();

        public List<StoreOverviewItem> StoresOverview { get; set; } = new();
    }

    public class StoreOverviewItem
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public bool IsCentralKitchen { get; set; }
        public int TotalOrders { get; set; }
        public int TotalInventoryItems { get; set; }
    }
}
