using ChemSecureApi.Model;
using Microsoft.AspNetCore.Identity;

namespace ChemSecureApi.DTOs
{
    public class UserDTO : IdentityUser
    {
        public string Address { get; set; }
        public List<Tank> Tanks { get; set; }
    }
}
