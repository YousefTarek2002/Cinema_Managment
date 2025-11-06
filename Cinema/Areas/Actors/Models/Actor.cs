
namespace Cinema.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ImgUrl { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
