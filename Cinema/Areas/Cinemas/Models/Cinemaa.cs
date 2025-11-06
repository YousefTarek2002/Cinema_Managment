
namespace Cinema.Models
{
    public class Cinemaa
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? ImgUrl { get; set; }
        public string? Logo { get; set; }
        public ICollection<Movie>? Movies { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
