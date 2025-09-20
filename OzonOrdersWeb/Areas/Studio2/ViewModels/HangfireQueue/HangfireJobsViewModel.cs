using Hangfire.Storage.Monitoring;
using Servcies.HangfireService.Models;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.HangfireQueue;

public class HangfireJobsViewModel
{
    public string QueueName { get; set; }
    public string ActiveTab { get; set; }
    public List<string> AvailableQueues { get; set; }
    public List<JobDto> EnqueuedJobs { get; set; }
    public List<JobDto> ProcessingJobs { get; set; }
    public List<JobDto> SucceededJobs { get; set; }
    public List<JobDto> DeletedJobs { get; set; } 
}