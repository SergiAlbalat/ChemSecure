using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace ChemSecureWeb.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IConfiguration _configuration;

        public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        [BindProperty]
        public RegisterDTO RegisterData { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            //  **Recuperar el token de autenticación**
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"Token enviado: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No se encontró un token de autenticación.");
                ModelState.AddModelError(string.Empty, "No tienes permisos para registrar usuarios.");
                return Page();
            }

            //  **Convertir el objeto a JSON**
            var jsonContent = JsonConvert.SerializeObject(RegisterData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //  **Realizar la solicitud HTTP**
            var response = await httpClient.PostAsync("api/Auth/register", content);

            //  **Manejo de respuestas**
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Usuario registrado con éxito.");
                return RedirectToPage("/Login");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("La API rechazó la solicitud por falta de permisos.");
                ModelState.AddModelError(string.Empty, "No tienes permisos para realizar esta acción.");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error en el registro: {response.StatusCode} - {responseContent}");
                ModelState.AddModelError(string.Empty, $"Error al registrar usuario: {responseContent}");
            }

            return Page();
        }
    }
}
