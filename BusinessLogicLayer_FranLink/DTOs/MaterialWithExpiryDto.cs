using System;

namespace BusinessLogicLayer_FranLink.DTOs
{
    /// <summary>
    /// Nguyên liệu với thông tin hạn sử dụng và lô
    /// </summary>
    public class MaterialWithExpiryDto
    {
        public Guid InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Quantity { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Status { get; set; } = string.Empty; // Normal, ExpiringSoon, Expired
    }
}
