using System;
using System.Collections.Generic;

namespace esdc_sa_appdev_reporting_api.Models
{
    public class TrelloCardDto
    {
        public string id { get; set; }
        public string idBoard { get; set; }
        public string idList { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public List<string> idLabels { get; set; }
        public List<string> idMembers { get; set; }


        public DateTime DateFromId 
        { 
            get
            {
                var timestamp = int.Parse(this.id.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
                
                var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                return dateTimeOffset.UtcDateTime;
            }
        }


        public TrelloCardDto()
        {
            idLabels = new List<string>();
            idMembers = new List<string>();
        }
    }
}