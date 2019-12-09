using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M32COM_CW.Services;

namespace M32COM_CW.Models.MemberViewModel
{
    public class MemberViewModel
    {
        public int? id { get; set; }
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Forename may only contain letters.")]
        [Required]
        [StringLength(30, ErrorMessage = "Forename is too long.")]

        public string Forename { get; set; }

        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Surname may only contain letters.")]
        [Required]
        [StringLength(30, ErrorMessage = "Surname too long.")]

        public string Surname { get; set; }
        public string Role { get; set; }

        public int? TeamID { get; set; }

        [Display(Name = "Profile Picture")]
        [ImageValidation(ErrorMessage = "Upload a jpg less than 1MB")]
        public IFormFile ProfilePicture { get; set; }

        public string FullName
        {
            get
            {
                return Forename + " " + Surname;
            }

        }

    }
}
