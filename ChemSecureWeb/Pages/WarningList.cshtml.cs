using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Linq;

namespace ChemSecureWeb.Pages
{
    public class WarningListModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<WarningDTO>? Warnings { get; set; } = new List<WarningDTO>();
        
        public WarningListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task OnGet()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("AuthToken"));
                
                
                var warnings = await client.GetFromJsonAsync<List<WarningDTO>>("api/Warning/warnings");
                
                if (warnings != null)
                {
                    // Sort by status and then by CreationDate.
                    Warnings = warnings.OrderByDescending(w => GetWarningPriority(w))
                                     .ThenByDescending(w => w.CreationDate)
                                     .ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error loading warnings: {ex.Message}");
                Warnings = new List<WarningDTO>();
            }
        }
        
        // Method to determinate the priority of the warnings.
        private int GetWarningPriority(WarningDTO warning)
        {
            var percentage = (warning.CurrentVolume / warning.Capacity) * 100;
            
            if (percentage >= 90) return 4; // Critical - High priority
            if (percentage >= 80) return 3;// High-Warning
            if (percentage >= 70) return 2;// Warning
            return 1; // Normal - Low priority
        }
    }
}
