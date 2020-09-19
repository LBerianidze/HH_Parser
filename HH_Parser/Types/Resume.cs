using System;
using System.Collections.Generic;

namespace HH_Parser
{
    public class Resume
    {
        public string OwnerName { get; set; }
        public string Link { get; set; }
        public string WorkingPeriod { get; set; }
        public string WorkingPosition { get; set; }
        public DateTime ResumeUpdate { get; set; }
        public DateTime ResumeCreated { get; set; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public bool WorkingNow { get; set; }
        public List<Work> WorkedPlaces { get; set; } = new List<Work>();
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsNew { get; set; }
        public string WorkingSummary { get; set; }
        public string Salary { get; set; }
        public string SalaryCurrency { get; set; }
        public string WorkingCity { get; set; }
        public string Age { get; set; }
        public string Position { get; set; }


    }
}
