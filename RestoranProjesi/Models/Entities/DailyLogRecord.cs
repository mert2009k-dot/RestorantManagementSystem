using System.ComponentModel.DataAnnotations;

namespace RestoranProjesi.Models.Entities
{
    public class DailyLogRecord
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string LogDate { get; set; } = string.Empty;
        
        public string? Note { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
