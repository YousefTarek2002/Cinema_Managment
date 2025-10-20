
using System.ComponentModel.DataAnnotations;

namespace Cinema.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Status { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string? MainImg { get; set; }
        public string? SubImages { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public int? CinemaId { get; set; }
        public Cinemaa? Cinema { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }
    }
}