using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
        public string UserName { get; private set; } = string.Empty;
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
                            _logger.LogInformation($"Retrieved {tanks.Count} tanks for user {UserName}");
                        }
                        else
                        {
                            _logger.LogWarning($"No tanks found for user {UserName}");
                           
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"API returned status code: {response.StatusCode}.");
                       
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error calling API: {ex.Message}");
                    
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error connecting to the server.";
                _logger.LogError(ex, "Error retrieving user tanks from API");
             
            }
        }

        public async Task<IActionResult> OnPostAddWarning(int tankId)
        {
            // Get user information and load tanks before processing the warning
            GetUserInfo();
            await GetUserTanksFromApiAsync();

            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to send warnings.";
                return RedirectToPage();
            }

            // Get tank information
            var tank = tanks.FirstOrDefault(t => t.Id == tankId);
            if (tank == null)
            {
                TempData["ErrorMessage"] = "The specified tank was not found.";
                return RedirectToPage();
            }

            // Verify that the tank is at least 65% full
            if (tank.Percentage < 65)
            {
                TempData["ErrorMessage"] = "Warnings can only be sent for tanks that are at least 65% full.";
                return RedirectToPage();
            }

            var client = _httpClientFactory.CreateClient("ChemSecureApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {              
                string clientName = UserName;



                if (string.IsNullOrEmpty(clientName))
                {
                    clientName = "User";
                    _logger.LogWarning("Could not get username, using default value");
                }
                // Create the DTO with the complete information
                var warningDTO = new
                {
                    ClientName = clientName,
                    Capacity = tank.Capacity,
                    CurrentVolume = tank.CurrentVolume,
                    TankId = tank.Id,
                    Type = tank.Type
                };

                var apiUrl = "api/Warning/add-warning";
                _logger.LogInformation("Sending POST request to {ApiUrl} to add warning for tank {TankId}", apiUrl, tankId);
                
                // Create the request content with the DTO
                var content = new StringContent(
                    JsonSerializer.Serialize(warningDTO),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Warning sent successfully for tank ID: {tankId}");
                    TempData["SuccessMessage"] = "Warning sent successfully! The management team has been notified.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error sending warning for tank ID {tankId}. Status: {response.StatusCode}. API Details: {errorContent}");

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        TempData["ErrorMessage"] = $"Error: Could not send the warning ({errorContent})";
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        TempData["ErrorMessage"] = "Authorization error. Please log in again.";
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        TempData["ErrorMessage"] = "Error: The warning service is not available.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Error registering the warning (Code: {response.StatusCode}).";
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Network error when sending warning for tank ID: {TankId}", tankId);
                TempData["ErrorMessage"] = "Connection error while trying to send the warning.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in OnPostAddWarning for tank ID: {TankId}", tankId);
                TempData["ErrorMessage"] = "Unexpected error while processing your warning.";
            }

            return RedirectToPage();
        }
        private void GetUserInfo()
        {
            try
            {
                // Get JWT token from session
                var token = HttpContext.Session.GetString("AuthToken");
                
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    
                    // Get user information from the token
                    var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    
                    // If ClaimTypes.Name is not available, try common alternative claim types
                    if (string.IsNullOrEmpty(usernameClaim))
                    {
                        usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name" || 
                                                                       c.Type == "username" || 
                                                                       c.Type == "preferred_username" || 
                                                                       c.Type == "sub")?.Value;
                    }
                    
                    // Log all available claims for debugging
                    _logger.LogInformation("Available claims in token: {Claims}", 
                        string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}:{c.Value}")));
                    
                    if (!string.IsNullOrEmpty(emailClaim))
                    {
                        UserEmail = emailClaim;
                    }
                    
                    if (!string.IsNullOrEmpty(nameClaim))
                    {
                        UserId = nameClaim;
                    }
                    
                    if (!string.IsNullOrEmpty(usernameClaim))
                    {
                        UserName = usernameClaim;
                        _logger.LogInformation("Username set to: {Username}", UserName);
                    }
                    else
                    {
                        // Fallback to email username part if no username claim is found
                        if (!string.IsNullOrEmpty(emailClaim) && emailClaim.Contains("@"))
                        {
                            UserName = emailClaim.Split('@')[0];
                            _logger.LogInformation("Username set from email: {Username}", UserName);
                        }
                        else
                        {
                            _logger.LogWarning("Could not determine username from token claims");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Unauthenticated user trying to access tanks page");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user information");
            }
        }
    }
}
