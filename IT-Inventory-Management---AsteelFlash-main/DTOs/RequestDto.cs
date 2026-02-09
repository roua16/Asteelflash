using System;

namespace ITStockM.DTOs
{
    public class RequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? Title { get; set; }
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? MaterialType { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
    }
}