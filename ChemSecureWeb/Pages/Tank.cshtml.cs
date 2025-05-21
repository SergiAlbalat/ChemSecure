using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChemSecureWeb.Pages
{
    public class TankModel : PageModel
    {
        [BindProperty]
        public int Percentage { get; set; }
        public void OnGet()
        {
            Percentage = 32;
        }
    }
}
