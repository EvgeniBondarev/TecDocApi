using Servcies.HangfireService.Models;

namespace Servcies.HangfireService;

public interface IHangfireService
{
    IEnumerable<string> GetAvailableQueues();
    List<JobDto> GetEnqueuedJobs(string queueName);
    List<JobDto> GetProcessingJobs(string queueName);
    List<JobDto> GetSucceededJobs();
    List<JobDto> GetDeletedJobs();
}