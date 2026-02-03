
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("DeliveryOrder", Schema = "dbo")]
    public partial class DeliveryOrder
    {
        [Key]
        [Required]
        public string DeleveryOrderNumber { get; set; }


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