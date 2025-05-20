using System.ComponentModel.DataAnnotations;

namespace ChemSecureWeb.Model
{
    public class RegisterDTO
    {

        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Insert username")]
        public string Name { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
