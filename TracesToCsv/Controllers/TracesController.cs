namespace TracesToCsv.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TracesController(IOptions<TracesOptions> options, ILogger<TracesController> logger) : Controller, ILoggable<TracesController>
{
    private static ConcurrentDictionary<Guid, ConcurrentBag<ServerTrace>> _traces = new();
    private static Timer? _traceTimer;

    ILogger<TracesController>? ILoggable<TracesController>.Logger { get; } = logger;

    [HttpGet("{id}")]
    public string Get(Guid id)
    {
        return id.ToString("N");
    }

    [HttpGet("{id}/key")]
    public string Key(Guid id) => CryptoUtilities.EncryptToString(id.ToString("N"), options.Value.Password);

    [HttpPut("{key}")]
    public unsafe string Create(string key, [FromBody] Trace trace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(trace);

        Span<char> chars = stackalloc char[128];
        if (!Uri.TryUnescapeDataString(key, chars, out var written))
            throw new ArgumentException(null, nameof(key));

        var guid = CryptoUtilities.Decrypt(chars.Slice(0, written).ToString(), options.Value.Password, 64); // validate key
        if (!Guid.TryParse(guid, out var id))
            throw new ArgumentException(null, nameof(key));

        var st = new ServerTrace(trace);
        _traces.AddOrUpdate(id, _ => [st], (_, list) =>
        {
            list.Add(st);
            return list;
        });

        Interlocked.CompareExchange(ref _traceTimer, new Timer(FlushTraces), null);
        _traceTimer.Change(options.Value.FlushTimeout, 0);

        return st.Id.ToString("N");
    }

    private static void FlushTraces(object? state)
    {
        Interlocked.Exchange(ref _traceTimer, null)?.Dispose();
        var traces = Interlocked.Exchange(ref _traces, new());
        EventProvider.Current.WriteMessage($"count: {traces.Count}");

        foreach (var kv in traces)
        {
            if (kv.Value.IsEmpty)
                continue;

            var ordered = kv.Value.OrderBy(t => t.Trace.Timestamp).ToArray();
        }
    }
}
