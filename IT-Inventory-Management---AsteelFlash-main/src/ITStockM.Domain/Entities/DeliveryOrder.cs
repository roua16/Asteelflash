
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITStockM.Domain.Base;

namespace ITStockM.Domain.Entities
{
    [Table("DeliveryOrder", Schema = "dbo")]
    public partial class DeliveryOrder : BaseEntity
    {
        [Required]
        public string DeliveryOrderNumber { get; set; }


        public string? OrderNumber { get; set; }

        [Required]
        public string? Descriptoin { get; set; }

        [Required]
        public string? SupplierName { get; set; }

        public Supplier? Supplier { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DeliveryDate { get; set; }

        public int EmployeeId { get; set; }

        public bool HasDelayedM { get; set; }

        public Employee? Employee { get; set; }

        public ICollection<DeliveryOrderMateriel> DeliveryOrderMateriels { get; set; } = new List<DeliveryOrderMateriel>();




    }
}