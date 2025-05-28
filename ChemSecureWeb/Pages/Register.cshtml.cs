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

            // **Retrieve authentication token**
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"Token sent: {token}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No authentication token found.");
                ModelState.AddModelError(string.Empty, "You do not have permission to register users.");
                return Page();
            }

            // **Convert the object to JSON**
            var jsonContent = JsonConvert.SerializeObject(RegisterData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // **Make the HTTP request**
            var response = await httpClient.PostAsync("api/Auth/register", content);

            // **Handle responses**
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User successfully registered.");
                return RedirectToPage("/Login");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("The API rejected the request due to lack of permissions.");
                ModelState.AddModelError(string.Empty, "You do not have permission to perform this action.");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Registration error: {response.StatusCode} - {responseContent}");
                ModelState.AddModelError(string.Empty, $"Error registering user: {responseContent}");
            }

            return Page();
        }
    }
}
