using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChemSecureWeb.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;

        [BindProperty]
        public LoginDTO Login { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClient, ILogger<LoginModel> logging)
        {
            _httpClient = httpClient;
            _logger = logging;
        }
        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClient.CreateClient("ChemSecureApi");
                var response = await client.PostAsJsonAsync("api/Auth/login", Login);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(token))
                    {
                        //Guardem en sessio (cookies) el Token amb la clau "AuthToken"
                        HttpContext.Session.SetString("AuthToken", token);

                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtToken = tokenHandler.ReadJwtToken(token); // Decodificar JWT
                        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                        if (!string.IsNullOrEmpty(usernameClaim))
                        {
                            HttpContext.Session.SetString("UserName", usernameClaim);
                        }
                        if (!string.IsNullOrEmpty(roleClaim))
                        {
                            HttpContext.Session.SetString("UserRole", roleClaim);
                        }
                        _logger.LogInformation("Login succesful");
                        Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = false });
                       // ModelState.AddModelError(string.Empty, "LOG IN SUCCESFULL");
                       //_logger.LogInformation($"Token obtenido: {token}"); Log to see if the token was correct and had all the information needed.
                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    _logger.LogInformation("Login failed");
                    ErrorMessage = "Incorrect information or unauthorized acces.";
                    ModelState.AddModelError(string.Empty, "LOG IN FAILED");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on login");
                ErrorMessage = "Unexpected error. Try again.";
            }

            return RedirectToPage("/Index");
        }
    }
}
