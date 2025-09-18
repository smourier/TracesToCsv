namespace TracesToCsv.Controllers;

public class ServerTrace(Trace trace)
{
    public Trace Trace { get; } = trace;
    public DateTimeOffset ReceivedUtcDate { get; } = DateTimeOffset.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
}
