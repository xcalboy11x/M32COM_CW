using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Models
{
    public class Entry
    {
        public Entry()
        {
        }

        public Entry(Team team, Event eventM, DateTime entryTS)
        {
            this.TeamID = team.ID;
            this.EventID = eventM.Id;
            this.EntryTimeStamp = entryTS;
            this.Team = team;
            this.Event = eventM;
        }
    
        public int ID { get; set; }
        public DateTime EntryTimeStamp { get; set; }
        public int TeamID { get; set; }
        public int EventID { get; set; }

        public Team Team { get; set; } = new Team();
        public Event Event { get; set; } = new Event();
    }
}
