using System.ComponentModel.DataAnnotations;

namespace RestoranProjesi.Models.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "İsim gereklidir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon gereklidir.")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Telefon numarası 10 veya 11 haneli olmalıdır.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Sadece rakam giriniz.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tarih gereklidir.")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Kişi sayısı gereklidir.")]
        public int PersonCount { get; set; }

        public string? Notes { get; set; }
        public string Status { get; set; } = "Bekliyor";
    }
}
