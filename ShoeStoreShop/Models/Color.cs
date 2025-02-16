namespace ShoeStore.Models
{
    public class Color
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Shoe> Shoes { get; set; }
    }
}
