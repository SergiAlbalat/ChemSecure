using Microsoft.AspNetCore.Identity;

namespace ChemSecureApi.Model
{
    public class User : IdentityUser
    {
        public string Address {  get; set; }
        public virtual ICollection<Tank> Tanks { get; set; } = new List<Tank>();
    }
}
