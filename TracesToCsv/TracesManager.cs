namespace TracesToCsv;

public sealed class TracesManager(
    IOptions<TracesOptions> options,
    ILogger<TracesManager> logger) : ILoggable<TracesManager>, IDisposable
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<ServerTrace>> _flushQueues = new();
    private readonly ConcurrentDictionary<long, Task> _flushTasks = [];
    private long _taskId;
    private ConcurrentDictionary<Guid, ConcurrentBag<ServerTrace>> _traces = new();
    private Timer? _mainTimer;

    ILogger<TracesManager>? ILoggable<TracesManager>.Logger => logger;

    public void Add(Guid userId, ServerTrace trace)
    {
        ArgumentNullException.ThrowIfNull(trace);
        if (userId == Guid.Empty) // prevent spam
            throw new ArgumentException(null, nameof(userId));

        _traces.AddOrUpdate(userId, _ => [trace], (_, list) =>
        {
            list.Add(trace);
            return list;
        });

        Interlocked.CompareExchange(ref _mainTimer, new Timer(FlushTraces), null);
        _mainTimer.Change(options.Value.FlushTimeout, 0);
    }

    public void Dispose()
    {
        var sw = Stopwatch.StartNew();
        FlushTraces(null);
        var array = _flushTasks.Values.ToArray();
        var ret = Task.WaitAll(array, options.Value.DisposeTimeout);
        this.LogInformation($"tasks: {array.Length} ret: {ret} elapsed: {sw.Elapsed}");
    }

    private void FlushTraces(object? state)
    {
        Interlocked.Exchange(ref _mainTimer, null)?.Dispose();
        var traces = Interlocked.Exchange(ref _traces, new());
        this.LogInformation($"count: {traces.Count}");

        foreach (var kv in traces)
        {
            if (kv.Value.IsEmpty)
                continue;

            _flushQueues.AddOrUpdate(kv.Key, kv.Value, (k, o) =>
            {
                // accumulate on existing
                foreach (var trace in kv.Value)
                {
                    o.Add(trace);
                }
                return o;
            });
        }

        foreach (var userId in traces.Keys)
        {
            if (_flushQueues.TryRemove(userId, out var bag))
            {
                var taskId = Interlocked.Increment(ref _taskId);
                var task = Task.Run(() => FlushQueue(taskId, userId, bag));
                _flushTasks[taskId] = task;
            }
        }
    }

    private void FlushQueue(long taskId, Guid guid, ConcurrentBag<ServerTrace> traces)
    {
        this.LogInformation($"guid: {guid} bag: {traces.Count} task id: {taskId}");
        // this runs on one thread only, ie: one file is written only by one thread

        // order traces by category (intermediate path)
        foreach (var group in traces.GroupBy(t => t.Category))
        {
            try
            {
                var category = group.Key;

                var dt = DateTime.UtcNow;
                var name = $"{dt:yyyy}_{dt:MM}_{dt:dd}.csv";
                var directoryPath = Path.GetFullPath(options.Value.DirectoryPath);
                var path = Path.Combine(directoryPath, guid.ToString("N"), category, name);
                IOUtilities.FileCreateDirectory(path);

                var i = 0;
                using var writer = new StreamWriter(path, true, Encoding.Unicode);
                var writerHeader = writer.BaseStream.Position == 0;
                foreach (var trace in group.OrderBy(t => t.Trace.Timestamp))
                {
                    if (i == 0 && writerHeader)
                    {
                        trace.WriteHeader(writer);
                        writer.WriteLine();
                        i++;
                    }

                    trace.Write(writer);
                    writer.WriteLine();
                }
                writer.Flush();
            }
            catch (Exception ex)
            {
                this.LogError("Error: " + ex);
            }
            finally
            {
                _flushTasks.Remove(taskId, out _);
            }
        }
    }
}
