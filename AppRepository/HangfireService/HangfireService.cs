using Hangfire;
using Hangfire.Storage;
using Servcies.HangfireService.Models;

namespace Servcies.HangfireService;

public class HangfireService : IHangfireService
{
    private readonly IMonitoringApi _monitoringApi;
    private const int MaxRecords = 1000; // Максимальное количество записей для выборки

    public HangfireService()
    {
        _monitoringApi = JobStorage.Current.GetMonitoringApi();
    }

    public IEnumerable<string> GetAvailableQueues()
    {
        return _monitoringApi.Queues().Select(q => q.Name);
    }

    public List<JobDto> GetEnqueuedJobs(string queueName)
    {
        var enqueuedJobs = _monitoringApi.EnqueuedJobs(queueName, 0, MaxRecords);
        var result = new List<JobDto>();

        foreach (var job in enqueuedJobs)
        {
            var jobId = job.Key;
            var stateData = JobStorage.Current.GetConnection().GetStateData(jobId);

            if (stateData != null && stateData.Name == "Deleted")
                continue;

            result.Add(new JobDto
            {
                Id = jobId,
                MethodName = job.Value.Job?.Method?.Name,
                EnqueuedAt = job.Value.EnqueuedAt,
                Args = job.Value.Job?.Args,
                State = "Enqueued"
            });
        }

        return result
            .OrderByDescending(j => j.EnqueuedAt)
            .Take(MaxRecords)
            .ToList();
    }

    public List<JobDto> GetProcessingJobs(string queueName)
    {
        var processingJobs = _monitoringApi.ProcessingJobs(0, MaxRecords)
            .Select(job => new JobDto
            {
                Id = job.Key,
                MethodName = job.Value.Job?.Method?.Name ?? "Unknown",
                StartedAt = job.Value.StartedAt,
                ServerId = job.Value.ServerId,
                Args = job.Value.Job?.Args,
                State = "Processing",
                Queue = job.Value.Job?.Queue
            })
            .OrderByDescending(j => j.StartedAt)
            .Take(MaxRecords)
            .ToList();

        return processingJobs;
    }

    public List<JobDto> GetSucceededJobs()
    {
        var failedJobs = _monitoringApi.FailedJobs(0, MaxRecords)
            .Select(j => new JobDto
            {
                Id = j.Key,
                MethodName = j.Value.Job?.Method?.Name,
                SucceededAt = j.Value.FailedAt,
                Duration = 0,
                Result = j.Value.ExceptionMessage,
                Args = j.Value.Job?.Args,
                State = "Failed"
            });

        var succeededJobs = _monitoringApi.SucceededJobs(0, MaxRecords)
            .Select(j => new JobDto
            {
                Id = j.Key,
                MethodName = j.Value.Job?.Method?.Name,
                SucceededAt = j.Value.SucceededAt,
                Duration = j.Value.TotalDuration,
                Result = (string)j.Value.Result,
                Args = j.Value.Job?.Args,
                State = "Succeeded"
            });

        return succeededJobs
            .Concat(failedJobs)
            .OrderByDescending(j => j.SucceededAt)
            .Take(MaxRecords)
            .ToList();
    }
    
    public List<JobDto> GetDeletedJobs()
    {
        var deletedJobs = _monitoringApi.DeletedJobs(0, MaxRecords)
            .Select(job => 
            {
                var jobData = JobStorage.Current.GetConnection().GetJobData(job.Key);
                return new JobDto
                {
                    Id = job.Key,
                    MethodName = jobData?.Job?.Method?.Name,
                    DeletedAt = job.Value.DeletedAt,
                    Args = jobData?.Job?.Args,
                    State = "Deleted",
                    Result = job.Value.LoadException?.Message,
                };
            })
            .OrderByDescending(j => j.DeletedAt)
            .Take(MaxRecords)
            .ToList();

        return deletedJobs;
    }
}