using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Models
{
    public class Boat
    {
        public Boat() : base()
        {
        }

        public Boat(Team team, int boatTeamId)
        {
            TeamId = boatTeamId;
            Team = team;
            Name = team.Name;
        }
        public int Id { get; set; }

        [Display(Name = "Boat Name")]
        public string Name { get; set; }

        [Display(Name = "Hull Length (mm)")]
        public Decimal HullLength { get; set; }

        [Display(Name = "Total Length (mm)")]
        public Decimal TotalLength { get; set; }

        [Display(Name = "Height (mm)")]
        public Decimal Height { get; set; }

        [Display(Name = "Battery Size (mAh)")]

        public string Battery { get; set; }

        public string Charger { get; set; }

        public string Transmitter { get; set; }

        public string Motor { get; set; }

        [Display(Name = "On Board Electronics")]
        public string OnBoardElectronics { get; set; }

        public int TeamId { get; set; }

        public virtual Team Team { get; set; }
    }
}
