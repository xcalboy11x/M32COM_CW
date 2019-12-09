using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using M32COM_CW.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace M32COM_CW.Models.EventViewModel
{
    public class EventViewModel
    {

        public int? Id { get; set; }
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "Description is too long.")]
        public string Description { get; set; }

        [Range(30, 600, ErrorMessage = "Duration must be between 30 minutes to 10 hours")]
        [Required]
        [Display(Name = "Duration(Minutes)")]
        public int DurationMinutes { get; set; }

        [Required]
        [Display(Name = "Event start date")]
        [DateValidation(ErrorMessage = "Event start date must be one day into the future.")]
        public DateTime? EventStartDateTime { get; set; }

        public int VenueId { get; set; }

        public Venue Venue { get; set; }

        [NotMapped]
        public List<SelectListItem> Venues { set; get; }

        [Display(Name = "Promotional Pictures")]
        [ImageValidation(ErrorMessage = "Upload a jpg less than 1MB")]
        public IFormFile PromoImage { get; set; }
    }
}
