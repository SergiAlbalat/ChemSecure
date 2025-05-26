using Microsoft.AspNetCore.Identity;

namespace ChemSecureApi.Tools
{
    public static class RoleTools
    {
        /// <summary>
        /// Method for creating the initial roles
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task CrearRolsInicials(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] rols = { "Admin", "User", "Manager" };

            foreach (var rol in rols)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }
    }
}
