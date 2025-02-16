using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Models
{
    public class ShoeImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Shoe_Id { get; set; }

        [Required, StringLength(255)]
        public string Image_Url { get; set; }

        public byte Status { get; set; } = 1; // 0: Inactive, 1: Active

        public DateTime Created_At { get; set; } = DateTime.Now;

        public Shoe Shoe { get; set; }
    }
}
