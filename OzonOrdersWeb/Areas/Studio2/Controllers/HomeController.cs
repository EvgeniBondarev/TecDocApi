using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OzonOrdersWeb.ViewModels;
using System.Diagnostics;
using System.Globalization;

namespace OzonOrdersWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            //var api = JobStorage.Current.GetMonitoringApi();
            //JobList<SucceededJobDto> succeededJobs = api.SucceededJobs(0, 50);

            //var results = succeededJobs.Select(job => JsonConvert.DeserializeObject<dynamic>((string)job.Value.Result)).ToList();

            //var jobResults = new List<JobResultViewModel>();

            //foreach (var result in results)
            //{
            //    var addedCount = (int)result["Item1"][0];
            //    var modifiedCount = (int)result["Item1"][1];
            //    var dateTimeParts = ((string)result["Item2"]).Split("-");

            //    var startTimeParts = dateTimeParts[0].Trim().Split(' ');
            //    var endTimeParts = dateTimeParts[1].Trim().Split(' ');

            //    var startTime = DateTime.ParseExact(startTimeParts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            //    var endTime = DateTime.ParseExact(endTimeParts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);

            //    var startTimeHourMinute = startTimeParts[1].Split(':');
            //    var endTimeHourMinute = endTimeParts[1].Split(':');

            //    startTime = startTime.AddHours(int.Parse(startTimeHourMinute[0])).AddMinutes(int.Parse(startTimeHourMinute[1]));
            //    endTime = endTime.AddHours(int.Parse(endTimeHourMinute[0])).AddMinutes(int.Parse(endTimeHourMinute[1]));

            //    var jobResult = new JobResultViewModel
            //    {
            //        AddedCount = addedCount,
            //        ModifiedCount = modifiedCount,
            //        ExecutionStartTime = startTime,
            //        ExecutionEndTime = endTime
            //    };
            //    jobResults.Add(jobResult);
            //}
            //return View(jobResults);
            return Redirect("/Orders/Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            // Получение сообщения об ошибке из HttpContext.Items
            ViewBag.ErrorMessage = HttpContext.Items["ErrorMessage"] as string;
            return View();
        }
    }
}
