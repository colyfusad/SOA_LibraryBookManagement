using AuthenAuthorService.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenAuthorService.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, 
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            //
            if (!roleManager.Roles.Any())
            {
                var roles = new string[] { UserRoles.Admin, UserRoles.Staff };
                foreach (var role in roles)
                {
                    var roleExist = await roleManager.FindByNameAsync(role);
                    if (roleExist == null)
                    {
                        var identityRole = new IdentityRole(role);
                        await roleManager.CreateAsync(identityRole);
                    }
                }
            }

            //
            if (!userManager.Users.Any())
            {
                var adminUser = new User
                {
                    UserName = "admin1234",
                };

                var adminResult = await userManager.CreateAsync(adminUser, "Admin@1234");
                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                }

                var staffUser = new User
                {
                    UserName = "user1234",
                };

                var landlordResult = await userManager.CreateAsync(staffUser, "Staff@1234");
                if (landlordResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, UserRoles.Staff);
                }
            }

            //
        }
    }
}
