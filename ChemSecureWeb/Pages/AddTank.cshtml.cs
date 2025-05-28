using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ChemSecureWeb.Pages
{
    public class AddTankModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        [BindProperty]
        public InsertTankDTO NewTank { get; set; } = new InsertTankDTO();
        
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;
        
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;

        public AddTankModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public IActionResult OnGet()
        {
            // Verificar si el usuario está autenticado
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }
            
            return Page();
        }
        
        private string GetUserIdFromToken()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Primero obtener el token y el ID del usuario
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "La sesión ha expirado. Por favor, inicie sesión nuevamente.";
                    return RedirectToPage("/Login");
                }

                // Obtener el ID del usuario autenticado
                var userId = GetUserIdFromToken();
                if (string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "No se pudo obtener la información del usuario autenticado.";
                    return Page();
                }


                // Asignar el ClientId al nuevo tanque ANTES de la validación
                NewTank.ClientId = userId;
                
                // Ahora validar el modelo
                if (!ModelState.IsValid)
                {
                    // Agregar errores de validación al ModelState
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            ErrorMessage = $"{modelStateKey}: {error.ErrorMessage}";
                            break;
                        }
                        if (!string.IsNullOrEmpty(ErrorMessage)) break;
                    }
                    return Page();
                }

                // Validar que el volumen actual no sea mayor que la capacidad
                if (NewTank.CurrentVolume > NewTank.Capacity)
                {
                    ErrorMessage = "El volumen actual no puede ser mayor que la capacidad del tanque.";
                    return Page();
                }


                // Asignar el ClientId al nuevo tanque
                NewTank.ClientId = userId;
                
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await client.PostAsJsonAsync("api/Tank", NewTank);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tanque creado exitosamente.";
                    return RedirectToPage("/Tank");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Error al crear el tanque: {response.StatusCode}";
                    
                    // Intentar deserializar el error como un objeto JSON
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        if (errorObj.TryGetProperty("errors", out var errors))
                        {
                            var errorList = new List<string>();
                            foreach (var error in errors.EnumerateObject())
                            {
                                foreach (var err in error.Value.EnumerateArray())
                                {
                                    errorList.Add($"{error.Name}: {err.GetString()}");
                                }
                            }
                            if (errorList.Any())
                            {
                                ErrorMessage = string.Join(" ", errorList);
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // Si no se puede deserializar como JSON, usar el mensaje de error original
                        ErrorMessage = $"Error al crear el tanque: {response.StatusCode} - {errorContent}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inesperado: {ex.Message}";
            }
            
            // Si llegamos aquí, hubo un error
            TempData["ErrorMessage"] = ErrorMessage;
            return Page();
        }
    }
}
