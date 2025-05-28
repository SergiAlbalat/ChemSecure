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
        public List<WarningDTO>? ManagedWarnings { get; set; } = new List<WarningDTO>();
        public string ActiveTab { get; set; } = "pending";
        
        public WarningListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task OnGet(string tab = "pending")
        {
            ActiveTab = tab;
            await LoadWarnings();
            await LoadManagedWarnings();
        }
        
        private async Task LoadWarnings()
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
        
        private async Task LoadManagedWarnings()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("AuthToken"));
                
                var managedWarnings = await client.GetFromJsonAsync<List<WarningDTO>>("api/Warning/managed-warnings");
                
                if (managedWarnings != null)
                {
                    // Sort by managed date
                    ManagedWarnings = managedWarnings.OrderByDescending(w => w.ManagedDate)
                                                  .ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error loading managed warnings: {ex.Message}");
                ManagedWarnings = new List<WarningDTO>();
            }
        }
        
        public async Task<IActionResult> OnPostManageWarningAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("AuthToken"));
                
                var response = await client.PutAsync($"api/Warning/manage/{id}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    // Handle error
                    return RedirectToPage(new { error = "Failed to manage warning" });
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error managing warning: {ex.Message}");
                return RedirectToPage(new { error = "An error occurred" });
            }
        }
        
        public async Task<IActionResult> OnPostUnmanageWarningAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ChemSecureApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("AuthToken"));
                
                var response = await client.PutAsync($"api/Warning/unmanage/{id}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage(new { tab = "managed" });
                }
                else
                {
                    // Handle error
                    return RedirectToPage(new { tab = "managed", error = "Failed to unmanage warning" });
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error unmanaging warning: {ex.Message}");
                return RedirectToPage(new { tab = "managed", error = "An error occurred" });
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
