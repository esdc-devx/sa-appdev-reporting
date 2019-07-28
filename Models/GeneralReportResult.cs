using System;
using System.Collections.Generic;

namespace esdc_sa_appdev_reporting_api.Models
{
    public class GeneralReportResult
    {
        public string CardId { get; set; }
        public string CardTitle { get; set; }
        public string CardCurrentListId { get; set; }
        public string CardCurrentListTitle { get; set; }
        public string CardUrl { get; set; }
        public List<LabelResult> CardLabels { get; set; }
        public Dictionary<string, string> CardMembers { get; set; }
        public DateTime? CardDateStarted { get; set; }
        public DateTime? CardDateCompleted { get; set; }
        public int NbrDaysOnHold { get; set; }


        public GeneralReportResult()
        {
            this.CardLabels = new List<LabelResult>();
            this.CardMembers = new Dictionary<string, string>();
        }
    }
}