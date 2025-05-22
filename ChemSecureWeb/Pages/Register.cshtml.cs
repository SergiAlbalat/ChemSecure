using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ChemSecureWeb.Pages
{
    public class RegisterModel : PageModel
    {
        // Propiedad para el nombre
        [BindProperty]
        [Required(ErrorMessage = "Name required")]
        public string Name { get; set; }

        // Propiedad para el email
        [BindProperty]
        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; }

        // Propiedad para la contraseña
        [BindProperty]
        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        // Propiedad para el teléfono
        [BindProperty]
        [Required(ErrorMessage = "Phone required")]
        [Phone(ErrorMessage = "Invalid phone")]
        public string Phone { get; set; }

        // Propiedad para la dirección
        [BindProperty]
        [Required(ErrorMessage = "Address required")]
        public string Address { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        // Maneja la solicitud GET: carga la página de registro
        public void OnGet()
        {
            // Puedes inicializar valores o limpiar datos previos aquí
        }

        // Maneja la solicitud POST: procesa el registro
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Aquí insertarías la lógica para registrar el usuario, por ejemplo:
            // - Validar que el email no esté registrado
            // - Encriptar la contraseña
            // - Guardar el usuario en la base de datos, etc.

            // Simulamos un registro exitoso:
            SuccessMessage = "Register completed, you can now login";
            return RedirectToPage("/Login");
        }
    }
}
