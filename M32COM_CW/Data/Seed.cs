using M32COM_CW.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M32COM_CW.Data
{
    public class Seed
    {
        public static async Task Initialize(M32COM_CWContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<Seed> logger)
        {
            context.Database.EnsureCreated();
            IdentityResult roleResult;
            string[] roleNames = { "Admin", "TeamManager", "Member", "TeamMember" };

            foreach (var roleName in roleNames)
            {
                // create the roles and seed database
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    // Creating default admin user
                    if (roleName == "Admin")
                        await CreateDefaultUserForApplication(context, userManager, roleManager, logger);
                }
            }
        }

        private static async Task CreateDefaultUserForApplication(M32COM_CWContext context, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, ILogger<Seed> logger)
        {
            const string email = "admin@test.com";

            var createdUser = await CreateDefaultUser(context, um, logger, email);
            await CreateDefaultMember(context, createdUser, logger, email);
            await SetPasswordForDefaultUser(um, logger, email, createdUser);
            await AddDefaultRoleToDefaultUser(um, logger, email, createdUser);
        }

        private static async Task<ApplicationUser> CreateDefaultUser(M32COM_CWContext context, UserManager<ApplicationUser> um, ILogger<Seed> logger, string email)
        {
            logger.LogInformation($"Create default user with email `{email}` for application");
            var user = new ApplicationUser(email);

            var ir = await um.CreateAsync(user);
            if (ir.Succeeded)
            {
                logger.LogDebug($"Created default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user `{email}` cannot be created");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            var createdUser = await um.FindByEmailAsync(email);
            return createdUser;
        }

        public static async Task CreateDefaultMember(M32COM_CWContext context, ApplicationUser user, ILogger<Seed> logger, string email)
        {
            Member member = new Member(user, "Admin", "User");

            context.Member.Add(member);
            await context.SaveChangesAsync();
        }

        private static async Task SetPasswordForDefaultUser(UserManager<ApplicationUser> um, ILogger<Seed> logger, string email, ApplicationUser user)
        {
            logger.LogInformation($"Set password for default user `{email}`");
            const string password = "Password123!";
            var ir = await um.AddPasswordAsync(user, password);
            if (ir.Succeeded)
            {
                logger.LogTrace($"Set password `{password}` for default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Password for the user `{email}` cannot be set");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static async Task AddDefaultRoleToDefaultUser(UserManager<ApplicationUser> um, ILogger<Seed> logger, string email, ApplicationUser user)
        {
            string[] administratorRoles = { "Admin", "Member" };
            logger.LogInformation($"Add default user `{email}` to role `{administratorRoles}`");
            var ir = await um.AddToRolesAsync(user, administratorRoles);
            if (ir.Succeeded)
            {
                logger.LogDebug($"Added the role `{administratorRoles}` to default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRoles}` cannot be set for the user `{email}`");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static string GetIdentiryErrorsInCommaSeperatedList(IdentityResult ir)
        {
            string errors = null;
            foreach (var identityError in ir.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }
            return errors;
        }
    }
}
