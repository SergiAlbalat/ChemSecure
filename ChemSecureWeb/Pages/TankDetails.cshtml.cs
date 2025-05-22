using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace ChemSecureWeb.Pages
{
    public class TankDetailsModel : PageModel
    {
        [BindProperty]
        public TankDTO tank {  get; set; }
        public async Task<IActionResult> OnGet(int? id)
        {
            if (id == null) return NotFound(); else return Page();
        }
    }
}
