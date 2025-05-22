using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChemSecureWeb.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IConfiguration _configuration;
        public RegisterModel(IHttpClientFactory httpClient, ILogger<RegisterModel> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
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

            var httpClient = _httpClient.CreateClient();
            httpClient.BaseAddress = new Uri(apiBaseUrl);

            var jsonContent = JsonConvert.SerializeObject(RegisterData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Login");
            }
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ModelState.AddModelError(string.Empty, "You are not authorized.");
            }

            ModelState.AddModelError(string.Empty, "Error while trying to register.");
            
            return Page();
        }
    }
}
