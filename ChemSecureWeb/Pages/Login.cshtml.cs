using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChemSecureWeb.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        // Esta propiedad se utiliza para mostrar mensajes de error al usuario
        [TempData]
        public string ErrorMessage { get; set; }

        // Maneja la solicitud GET: cuando se carga la página
        public void OnGet()
        {
            // Puedes inicializar algo si es necesario
        }

        // Maneja la solicitud POST: cuando se envía el formulario
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Lógica simple de autenticación para demostración:
            // En un escenario real, reemplaza esta comparación con una búsqueda en tu base de datos
            if (Username == "admin" && Password == "password")
            {
                // Autenticación exitosa, redirige al usuario a la página principal o dashboard
                return RedirectToPage("/Index");
            }
            else
            {
                // Si falla la autenticación, se agrega el error al ModelState para mostrarlo en el formulario
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return Page();
            }
        }
    }
}
