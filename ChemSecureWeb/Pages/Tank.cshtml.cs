using ChemSecureWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChemSecureWeb.Pages
{
    public class TankModel : PageModel
    {
        public List<TankDTO> tanks = new List<TankDTO>();
            
        public void OnGet()
        {
            tanks.Add(new TankDTO { Capacity = 6, CurrentVolume = 2.4 });
            tanks.Add(new TankDTO { Capacity = 20, CurrentVolume = 13 });
            tanks.Add(new TankDTO { Capacity = 20, CurrentVolume = 20 });
            tanks.Add(new TankDTO { Capacity = 20, CurrentVolume = 0 });
        }
    }
}
