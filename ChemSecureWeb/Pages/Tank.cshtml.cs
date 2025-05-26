using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ChemSecureWeb.Pages
{
    public class TankModel : PageModel
    {
        private readonly ILogger<TankModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public List<TankDTO> tanks = new List<TankDTO>();
        public string UserEmail { get; private set; } = string.Empty;
        public string UserId { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        
        public TankModel(ILogger<TankModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
            
        public async Task OnGetAsync()
        {
            GetUserInfo();
            await GetUserTanksFromApiAsync();
        }
        
        private async Task GetUserTanksFromApiAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "You must be logged in to view your tanks.";
                    CreateSampleTanks();
                    return;
                }
                
                try
                { 
                    var client = _httpClientFactory.CreateClient("ChemSecureApi");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    // Realizar la petición a la API
                    var response = await client.GetAsync("api/Tank/user");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer y deserializar la respuesta
                        var content = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        
                        var userTanks = JsonSerializer.Deserialize<List<TankDTO>>(content, options);
                        
                        if (userTanks != null && userTanks.Any())
                        {
                            tanks = userTanks;
                            _logger.LogInformation($"Retrieved {tanks.Count} tanks for user {UserEmail}");
                        }
                        else
                        {
                            _logger.LogWarning($"No tanks found for user {UserEmail}");
                            CreateSampleTanks();
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"API returned status code: {response.StatusCode}. Using sample tanks.");
                        // Si hay un error con la API, crear tanques de ejemplo
                        CreateSampleTanks();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error calling API: {ex.Message}. Using sample tanks.");
                    // Si hay una excepción al llamar a la API, crear tanques de ejemplo
                    CreateSampleTanks();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error connecting to the server.";
                _logger.LogError(ex, "Error retrieving user tanks from API");
                // En caso de error, crear tanques de ejemplo
                CreateSampleTanks();
            }
        }
        
        private void CreateSampleTanks()
        {
            // Crear tanques de ejemplo con el usuario actual
            tanks.Add(new TankDTO { Id = 1, Capacity = 6, CurrentVolume = 2.4, Type = residusType.Acids, UserId = UserId, UserEmail = UserEmail });
            tanks.Add(new TankDTO { Id = 2, Capacity = 20, CurrentVolume = 13, Type = residusType.AqueousSolutions, UserId = UserId, UserEmail = UserEmail });
            tanks.Add(new TankDTO { Id = 3, Capacity = 20, CurrentVolume = 20, Type = residusType.HalogenatedSolvents, UserId = UserId, UserEmail = UserEmail });
            tanks.Add(new TankDTO { Id = 4, Capacity = 20, CurrentVolume = 0, Type = residusType.NonHalogenatedSolvents, UserId = UserId, UserEmail = UserEmail });
        }
        
        private void GetUserInfo()
        {
            try
            {
                // Obtener el token JWT de la sesión
                var token = HttpContext.Session.GetString("AuthToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    
                    // Obtener el correo electrónico del usuario desde el token
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    
                    if (!string.IsNullOrEmpty(emailClaim))
                    {
                        UserEmail = emailClaim;
                    }
                    
                    if (!string.IsNullOrEmpty(nameClaim))
                    {
                        UserId = nameClaim;
                    }
                }
                else
                {
                    _logger.LogWarning("Unauthenticated user trying to access tanks page");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario");
            }

        }
    }
}
