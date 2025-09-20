using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// Controllers/HangfireQueueController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using OzonOrdersWeb.Areas.Studio2.ViewModels.HangfireQueue;
using Servcies.HangfireService;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class HangfireQueueController : Controller
{
    private readonly IHangfireService _hangfireService;

    public HangfireQueueController(IHangfireService hangfireService)
    {
        _hangfireService = hangfireService;
    }

    public IActionResult Index(string queueName = "upload-queue-new", string tab = "enqueued")
    {
        var queues = _hangfireService.GetAvailableQueues();
        var model = new HangfireJobsViewModel
        {
            QueueName = queueName,
            ActiveTab = tab,
            AvailableQueues = queues.ToList(),
            
            EnqueuedJobs = _hangfireService.GetEnqueuedJobs(queueName),
            ProcessingJobs = _hangfireService.GetProcessingJobs(queueName),
            SucceededJobs = _hangfireService.GetSucceededJobs(),
            DeletedJobs = _hangfireService.GetDeletedJobs() 
            
        };

        return View(model);
    }
    
    [HttpPost]
    public IActionResult DeleteJob([FromBody] DeleteJobRequest request)
    {
        if (string.IsNullOrEmpty(request.JobId))
            return BadRequest();

        var success = BackgroundJob.Delete(request.JobId);

        if (success)
            return Ok();
        else
            return NotFound(); // Задание не найдено или уже завершено
    }
}