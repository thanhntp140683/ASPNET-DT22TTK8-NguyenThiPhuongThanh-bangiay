using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class Size
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal SizeNumber { get; set; }

        public byte Status { get; set; } = 1;
    }
}
