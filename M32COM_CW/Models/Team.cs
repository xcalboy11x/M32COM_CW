using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Models
{
    public class Team
    {
        public int ID { get; set; }

        [Display(Name = "Team Name")]
        [Required]
        public string Name { get; set; }
        public int TeamLeaderId { get; set; }

        public ICollection<Entry> Entries { get; set; }

        public ICollection<Member> Members { get; set; } = new List<Member>();
        public virtual Boat Boat { get; set; } = new Boat();
    }
}
