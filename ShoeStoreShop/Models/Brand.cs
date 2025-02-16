using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public byte Status { get; set; } = 1; // 0: Inactive, 1: Active

        public ICollection<Shoe> Shoes { get; set; } = new List<Shoe>();
    }
}
