using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        // المستخدم صاحب السلة
        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        // الفيلم في السلة
        [Required]
        public int MovieId { get; set; }

        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; }

        // عدد النسخ أو التذاكر
        [Required]
        [Range(1, int.MaxValue)]
        public int Count { get; set; } = 1;

        // السعر الحالي للفيلم
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // وقت الإضافة
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
