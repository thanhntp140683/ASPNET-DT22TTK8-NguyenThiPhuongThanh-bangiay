using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public byte Status { get; set; } = 1; // 0: Inactive, 1: Active

        public ICollection<Shoe> Shoe { get; set; } = new List<Shoe>();
    }
}
