namespace Servcies.HangfireService.Models;

public class JobDto
{
    public string Id { get; set; }
    public string MethodName { get; set; }
    public string State { get; set; }
    public object Args { get; set; }
    public string Queue { get; set; }
    
    public DateTime? EnqueuedAt { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string ServerId { get; set; }
    
    public DateTime? SucceededAt { get; set; }
    public long? Duration { get; set; }
    public string Result { get; set; }
}