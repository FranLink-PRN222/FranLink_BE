using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer_FranLink.DTOs
{
    public class InventoryDashboardSummary
    {
        public int TotalCentralKitchenItems { get; set; }
        public int TotalStoreItems { get; set; }
        public int LowStockCount { get; set; }
        public int OverstockCount { get; set; }
        public int ExpiringCount { get; set; }
        public int ExpiredCount { get; set; }
        public int PendingTransfers { get; set; }
        public int PendingDisposals { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }
}
