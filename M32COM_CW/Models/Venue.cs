using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Models
{
    public class Venue
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Address line is too long.")]

        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        [StringLength(30, ErrorMessage = "Town is too long.")]

        [Required]
        public string Town { get; set; }

        [StringLength(8, ErrorMessage = "Postcode is too long.")]
        [Required]
        public string Postcode { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
