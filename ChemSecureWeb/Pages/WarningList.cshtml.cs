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
                
                // Obtener las advertencias de la API
                var warnings = await client.GetFromJsonAsync<List<WarningDTO>>("api/Warning/warnings");
                
                if (warnings != null)
                {
                    // Ordenar por prioridad (status) y luego por fecha
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
        
        // Método auxiliar para determinar la prioridad de una advertencia
        private int GetWarningPriority(WarningDTO warning)
        {
            var percentage = (warning.CurrentVolume / warning.Capacity) * 100;
            
            if (percentage >= 90) return 3; // Critical - Mayor prioridad
            if (percentage >= 75) return 2; // Warning
            return 1; // Info - Menor prioridad
        }
    }
}
