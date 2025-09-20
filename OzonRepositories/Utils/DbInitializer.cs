using Microsoft.AspNetCore.Identity;
using OzonRepositories.Context.Identity;

namespace OzonRepositories.Utils
{
    public class DbInitializer
    {
        private readonly OzonIdentityOrderContext _IdentityContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<CustomIdentityUser> _userManager;

        public DbInitializer(OzonIdentityOrderContext context,
                            RoleManager<IdentityRole> roleManager,
                            UserManager<CustomIdentityUser> userManager)
        {
            _IdentityContext = context;
            _roleManager = roleManager;
            _userManager = userManager;

        }

        public void InitializeDb()
        {
            if (!_roleManager.Roles.Any())
            {
                CreateRoles().Wait();
            }

            CreateAdmin().Wait();

            _IdentityContext.SaveChanges();
        }

        private async Task CreateRoles()
        {
            string[] roleNames = { "Admin", "User", };

            foreach (var roleName in roleNames)
            {

                var roleExist = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task CreateAdmin()
        {
            var isAdmin = (await _userManager.FindByEmailAsync("admin@gmail.com"));
            if (isAdmin == null)
            {
                CustomIdentityUser user = new CustomIdentityUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com"
                };

                IdentityResult result = await _userManager.CreateAsync(user, "EQRu~ha+75hqIcr");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }


    }
}
