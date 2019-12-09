using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace M32COM_CW.Models
{
    public class Member
    {
        public Member()
        {
        }

        public Member(ApplicationUser applicationUser, string forename, string surname)
        {
            ApplicationUser = applicationUser;
            ApplicationUserId = applicationUser.Id;
            Surname = surname;
            Forename = forename;
        }

        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("TeamID")]
        public int? TeamID { get; set; }

        public Team Team { get; set; }

        public string Forename { get; set; }

        public string Surname { get; set; }
        public string Role { get; set; }
        public byte[] ProfilePicture { get; set; }

        public string FullName
        {
            get
            {
                return Forename + " " + Surname;
            }

        }

    }
}
