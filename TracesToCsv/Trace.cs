namespace TracesToCsv;

public sealed class Trace
{
    public required TraceVersion Version { get; set; }
    public required TraceLevel Level { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required string Category { get; set; }
    public string? Message { get; set; }
    public string? Id { get; set; }
    public IDictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
}
