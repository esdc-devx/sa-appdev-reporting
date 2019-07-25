using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using esdc_sa_appdev_reporting_api.Services;

namespace esdc_sa_appdev_reporting_api
{
    public static class TrelloReportingApi
    {
        [FunctionName("getGeneralReportResults")]
        public static async Task<IActionResult> GetGeneralReportResults
        (
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            var service = new TrelloService();

            var results = await service.GetGeneralReportResults();

            return results != null
                ? (ActionResult) new OkObjectResult(results)
                : new BadRequestObjectResult("No results retrieved...");
        }


        [FunctionName("getSummaryReportResults")]
        public static async Task<IActionResult> GetSummaryReportResults
        (
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log
        )
        {
            var service = new TrelloService();

            var results = await service.GetSummaryReportResults();

            return results != null
                ? (ActionResult) new OkObjectResult(results)
                : new BadRequestObjectResult("No results retrieved...");
        }
    }
}