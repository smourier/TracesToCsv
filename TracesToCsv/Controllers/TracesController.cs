namespace TracesToCsv.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TracesController(IOptions<TracesOptions> options, ILogger<TracesController> logger) : Controller, ILoggable<TracesController>
{
    private static ConcurrentDictionary<string, ConcurrentBag<ServerTrace>> _traces = new();
    ILogger<TracesController>? ILoggable<TracesController>.Logger { get; } = logger;

    [HttpGet("{id}")]
    public string Get(Guid id)
    {
        return id.ToString("N");
    }

    [HttpGet("{id}/key")]
    public string Key(Guid id)
    {
        return CryptoUtilities.EncryptToString(id.ToString("N"), options.Value.Password);
    }

    [HttpPut("{key}")]
    public string Create(string key, [FromBody] Trace trace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(trace);
        this.LogTrace($"Received trace for key '{key}' list:{string.Join(", ", _traces.Select(kv => kv.Key + "=" + kv.Value.Count))}");
        var st = new ServerTrace(trace);

        _traces.AddOrUpdate(key, _ => [st], (_, list) =>
        {
            list.Add(st);
            return list;
        });

        return st.Id.ToString("N");
    }
}
