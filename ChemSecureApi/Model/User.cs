using Microsoft.AspNetCore.Identity;

namespace ChemSecureApi.Model
{
    public class User : IdentityUser
    {
        public string Address {  get; set; }
        public List<Tank>? Tanks { get; set; }
    }
}
