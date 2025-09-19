namespace TracesToCsv.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TracesController(
    TracesManager manager,
    IOptions<TracesOptions> options,
    ILogger<TracesController> logger) : Controller, ILoggable<TracesController>
{
    ILogger<TracesController>? ILoggable<TracesController>.Logger { get; } = logger;

    [HttpGet("{id}/key")]
    public string Key(Guid id) => manager.GetKey(id);

    [HttpPut("{key}/{*url}")]
    public unsafe string AddTrace(string key, string? url, [FromBody] Trace trace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(trace);

        var guid = CryptoUtilities.Decrypt(key, options.Value.Password, 128); // validate key
        if (!Guid.TryParse(guid, out var userId))
            throw new ArgumentException(null, nameof(key));

        trace.Category ??= url;
        var st = new ServerTrace(trace);
        manager.Add(userId, st);
        return st.Id.ToString("N");
    }
}
