using System.Collections.Generic;

namespace esdc_sa_appdev_reporting_api.Models
{
    public class SummaryReportResult
    {
        public List<List<string>> CompiledResults { get; set; }
        public List<GeneralReportResult> RawResults { get; set; }


        public SummaryReportResult()
        {
            this.CompiledResults = new List<List<string>>();
            this.RawResults = new List<GeneralReportResult>();
        }
    }
}