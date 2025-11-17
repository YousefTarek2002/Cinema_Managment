namespace Cinema.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        public string Code { get; set; }
        public decimal Discount { get; set; } // % أو مبلغ ثابت لاحقًا نضبطها

        public bool IsValid { get; set; }
        public DateTime ValidTo { get; set; }

        public int MaxUsage { get; set; }
        public int UsedCount { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
