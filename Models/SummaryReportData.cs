using System.Collections.Generic;

namespace esdc_sa_appdev_reporting_api.Models
{
    public class SummaryReportData
    {
        public List<List<string>> CompiledResults { get; set; }
        public List<SummaryReportItemResult> ReportItemResults { get; set; }


        public SummaryReportData()
        {
            this.CompiledResults = new List<List<string>>();
            this.ReportItemResults = new List<SummaryReportItemResult>();
        }
    }
}