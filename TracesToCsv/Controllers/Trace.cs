namespace TracesToCsv.Controllers;

public class Trace
{
    public required TraceVersion Version { get; set; }
    public required TraceLevel Level { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public string? Category { get; set; }
    public string? Id { get; set; }
    public IDictionary<string, object?> Values { get; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
}
