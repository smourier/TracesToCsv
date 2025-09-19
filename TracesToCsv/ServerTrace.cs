namespace TracesToCsv;

public sealed class ServerTrace(Trace trace)
{
    public Trace Trace { get; } = trace;
    public DateTimeOffset ReceivedUtcDate { get; } = DateTimeOffset.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
    public string Category { get; } = TracesManager.ComputeSafeCategory(trace.Category);

    internal void WriteHeader(TextWriter writer)
    {
        if (writer == null)
            return;

        writer.WriteCsvCell(nameof(Id));
        writer.WriteCsvCell(nameof(ReceivedUtcDate));
        writer.WriteCsvCell(nameof(Category));
        writer.WriteCsvCell(nameof(Trace.Version));
        writer.WriteCsvCell(nameof(Trace.Level));
        writer.WriteCsvCell(nameof(Trace.Timestamp));
        writer.WriteCsvCell("TraceId");

        if (Trace.Values.Count > 0)
        {
            writer.WriteCsvCell(nameof(Trace.Message));
            var keys = Trace.Values.Keys.Order().ToArray();
            for (var i = 0; i < keys.Length - 1; i++)
            {
                writer.WriteCsvCell(keys[i]);
            }
            writer.WriteCsvCell(keys[^1], false);
        }
        else
        {
            writer.WriteCsvCell(nameof(Trace.Message), false);
        }
    }

    internal void Write(TextWriter writer)
    {
        if (writer == null)
            return;

        writer.WriteCsvCell(Id.ToString("N"));
        writer.WriteCsvCell(ReceivedUtcDate.ToString("O"));
        writer.WriteCsvCell(Category);
        writer.WriteCsvCell(Trace.Version.ToString());
        writer.WriteCsvCell(Trace.Level.ToString());
        writer.WriteCsvCell(Trace.Timestamp.ToString("O"));
        writer.WriteCsvCell(Trace.Id?.ToString());

        if (Trace.Values.Count > 0)
        {
            writer.WriteCsvCell(Trace.Message);
            var keys = Trace.Values.Keys.Order().ToArray();
            for (var i = 0; i < keys.Length - 1; i++)
            {
                writer.WriteCsvCell(ToString(Trace.Values[keys[i]]));
            }
            writer.WriteCsvCell(ToString(Trace.Values[keys[^1]]), false);
        }
        else
        {
            writer.WriteCsvCell(Trace.Message, false);
        }
    }

    private static string? ToString(object? value)
    {
        if (value == null)
            return null;

        return string.Format("{0}", value).Nullify();
    }
}
