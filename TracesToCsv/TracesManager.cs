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

    public string DirectoryPath { get; } = Path.GetFullPath(options.Value.DirectoryPath);

    public string GetKey(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(null, nameof(id));

        return CryptoUtilities.EncryptToString(id.ToString("N"), options.Value.Password);
    }

    public TraceEntry? GetEntry(Guid id, string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var relativePath = ComputeSafeCategory(url) ?? string.Empty;
        var root = relativePath.Length == 0;

        var name = id.ToString("N");
        var path = Path.Combine(DirectoryPath, name, relativePath);
        var fullName = Path.Combine(name, relativePath);
        var parentFullName = root ? string.Empty : Path.GetDirectoryName(Path.Combine(name, relativePath))!;

        // we currently only support csv files download
        var ext = Path.GetExtension(path);
        if (ext.EqualsIgnoreCase(".csv"))
        {
            var fi = new FileInfo(path);
            if (!fi.Exists)
                return null;

            return new TraceFile
            {
                FullName = fullName,
                LastWriteTimeUtc = fi.LastWriteTimeUtc,
                Length = fi.Length,
                ParentFullName = parentFullName,
                FilePath = path
            };
        }

        var folder = new TraceFolder { FullName = fullName, IsRoot = root, ParentFullName = parentFullName };
        if (!Directory.Exists(path))
            return root ? folder : null;

        Build(folder, path);
        return folder;
    }

    public void Dispose()
    {
        var sw = Stopwatch.StartNew();
        FlushTraces(null);
        var array = _flushTasks.Values.ToArray();
        var ret = Task.WaitAll(array, options.Value.DisposeTimeout);
        this.LogInformation($"tasks: {array.Length} ret: {ret} elapsed: {sw.Elapsed}");
    }

    private void Build(TraceFolder parent, string path)
    {
        try
        {
            var options = new EnumerationOptions();
            foreach (var dir in Directory.EnumerateDirectories(path, "*", options))
            {
                var child = new TraceFolder
                {
                    FullName = Path.Combine(parent.FullName, Path.GetFileName(dir)),
                    IsRoot = false,
                    ParentFullName = parent.FullName
                };
                parent.Entries.Add(child);
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.csv", options))
            {
                var fi = new FileInfo(file);
                var child = new TraceFile
                {
                    FullName = Path.Combine(parent.FullName, fi.Name),
                    LastWriteTimeUtc = fi.LastWriteTimeUtc,
                    Length = fi.Length,
                    ParentFullName = parent.FullName
                };
                parent.Entries.Add(child);
            }
        }
        catch (Exception ex)
        {
            this.LogError("Error: " + ex);
        }
    }

    internal static string ComputeSafeCategory(string? category)
    {
        category = category.Nullify();
        if (category == null)
            return string.Empty;

        var segments = category.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join('\\', segments.Select(s => IOUtilities.NameToValidFileName(s) ?? string.Empty));
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
                var path = Path.Combine(DirectoryPath, guid.ToString("N"), category, name);
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
