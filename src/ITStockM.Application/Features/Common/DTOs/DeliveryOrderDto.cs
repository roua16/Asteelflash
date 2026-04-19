using System;

namespace ITStockM.Application.Features.Common.DTOs
{
    public class DeliveryOrderDto
    {
        public string DeliveryOrderNumber { get; set; }
        public string? OrderNumber { get; set; }
        public string? Descriptoin { get; set; }
        public string? SupplierName { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int EmployeeId { get; set; }
    }
}