using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class Shoe
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "DECIMAL(10,0)")]
        public decimal Price { get; set; }

        public int Category_Id { get; set; }
        public Category Category { get; set; }
        public int Brand_Id { get; set; }
        public Brand Brand { get; set; }


        [Required, StringLength(10)]
        public string Gender { get; set; } // Men, Women, Unisex


        public byte Status { get; set; } = 1; // 0: Inactive, 1: Active

        public int ColorId { get; set; }
        public Color Color { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
        public DateTime? Updated_At { get; set; }

        public ICollection<ShoeImage> ShoeImages { get; set; } = new List<ShoeImage>();
        [Required, Column(TypeName = "INT")]
        public int Amount { get; set; }
    }
}
