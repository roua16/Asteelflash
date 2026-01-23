
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITStockM.Models.ITStockManagment
{
    [Table("Employee", Schema = "dbo")]
    public partial class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Post { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Service { get; set; }

        public ICollection<Assignment> Assignments { get; set; }

        public ICollection<Request> Requests { get; set; }

        public ICollection<DeliveryOrder> DeliveryOrders { get; set; }
    }
}