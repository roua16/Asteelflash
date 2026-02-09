using System;

namespace ITStockM.DTOs
{
    public class MaterielDto
    {
        public int Id { get; set; }
        public string MaterielName { get; set; }
        public string Type { get; set; }
        public string? SerialNumber { get; set; }
        public int QuantityITStock { get; set; }
        public int QuantityPDRStock { get; set; }
        public int IrreparableQuantity { get; set; }
        public int Repairing_Quantity { get; set; }
        public DateTime Warranty { get; set; }
    }
}