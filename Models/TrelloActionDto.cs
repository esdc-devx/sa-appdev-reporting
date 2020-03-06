using System;

namespace esdc_sa_appdev_reporting_api.Models
{
    public class TrelloActionDto
    {
        public string id { get; set; }
        public string type { get; set; }
        public DateTime date { get; set; }
        public TrelloActionDataDto data { get; set; }


        public DateTime DateFromId 
        { 
            get
            {
                var timestamp = int.Parse(this.id.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
                
                var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);

                return dateTimeOffset.UtcDateTime;
            }
        }


        public TrelloActionDto()
        {
            data = new TrelloActionDataDto();            
        }


        public bool IsListTransfer ()
        {
            if ((string.IsNullOrEmpty(data.listAfter.id) == false) &&
                (string.IsNullOrEmpty(data.listBefore.id) == false))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}