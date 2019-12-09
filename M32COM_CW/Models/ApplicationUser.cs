
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace M32COM_CW.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
          public ApplicationUser()
        : base()
        {
        }

        public ApplicationUser(string userName)
            : base(userName)
        {
            base.Email = userName;
        }

        public string GetUserId()
        {
            return Id;
        }
    }
}
