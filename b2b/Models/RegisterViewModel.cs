using System.ComponentModel.DataAnnotations;

namespace b2b.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Şirket adresi zorunludur.")]
        public string CompanyAddress { get; set; }

        [Required(ErrorMessage = "Şirket telefonu zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string CompanyPhone { get; set; }
    }
}