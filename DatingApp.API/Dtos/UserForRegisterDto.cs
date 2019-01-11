using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8,MinimumLength=4, ErrorMessage="Şifre 8 ila 4 karakter arasında olmalıdır.")]
        public string Password { get; set; }

    }
}