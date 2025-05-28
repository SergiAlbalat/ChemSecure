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
            // Check if user is authenticated
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }
            SuccessMessage = TempData["SuccessMessage"] as string;

            return Page();
        }
        
      
        
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // First, get the token
                var token = HttpContext.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(token))
                {
                    ErrorMessage = "Your session has expired. Please log in again.";
                    return RedirectToPage("/Login");
                }

                // Validate that a ClientId has been provided
                if (string.IsNullOrEmpty(NewTank.ClientId))
                {
                    ErrorMessage = "Client ID is required.";
                    return Page();
                }
                
                // Now validate the model
                if (!ModelState.IsValid)
                {
                    // Add validation errors to ModelState
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

                // Validate that current volume is not greater than capacity
                if (NewTank.CurrentVolume > NewTank.Capacity)
                {
                    ErrorMessage = "Current volume cannot be greater than tank capacity.";
                    return Page();
                }


                // ClientId is already assigned from the form
                
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await client.PostAsJsonAsync("api/Tank", NewTank);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tank created successfully.";

                    return RedirectToPage("/AddTank");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Error creating tank: {response.StatusCode}";
                    
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
                        // If cannot deserialize as JSON, use the original error message
                        ErrorMessage = $"Error creating tank: {response.StatusCode} - {errorContent}";
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Connection error: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected error: {ex.Message}";
            }
            
            // If we got here, there was an error
            TempData["ErrorMessage"] = ErrorMessage;
            return Page();
        }
    }
}
