using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ChemSecureWeb.Pages
{
    public class TankDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public TankDetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        [BindProperty]
        public TankDTO Tank {  get; set; }
        public async Task<IActionResult> OnGet(int? id)
        {
            if (id == null) return NotFound();
            var client = _httpClientFactory.CreateClient("ChemSecureApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("AuthToken"));
            Tank = await client.GetFromJsonAsync<TankDTO>($"api/Tank/{id}");
            return Page();
        }
    }
}
