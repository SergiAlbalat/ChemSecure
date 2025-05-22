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

        // Maneja la solicitud GET: cuando se carga la p�gina
        public void OnGet()
        {
            // Puedes inicializar algo si es necesario
        }

        // Maneja la solicitud POST: cuando se env�a el formulario
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // L�gica simple de autenticaci�n para demostraci�n:
            // En un escenario real, reemplaza esta comparaci�n con una b�squeda en tu base de datos
            if (Username == "admin" && Password == "password")
            {
                // Autenticaci�n exitosa, redirige al usuario a la p�gina principal o dashboard
                return RedirectToPage("/Index");
            }
            else
            {
                // Si falla la autenticaci�n, se agrega el error al ModelState para mostrarlo en el formulario
                ModelState.AddModelError(string.Empty, "Usuario o contrase�a incorrectos.");
                return Page();
            }
        }
    }
}
