namespace esdc_sa_appdev_reporting_api.Models
{
    public class SummaryReportItemResult
    {
        public string TaskName { get; set; }
        public string ClientName { get; set; }
        public string StatusTitle { get; set; }
        public string DateStarted { get; set; }
        public string DateCompleted { get; set; }
        public string AssignedTo { get; set; }
        public string Url { get; set; }
    }
}