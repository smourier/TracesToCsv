namespace TracesToCsv;

public sealed class ServerTrace(Trace trace)
{
    public Trace Trace { get; } = trace;
    public DateTimeOffset ReceivedUtcDate { get; } = DateTimeOffset.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
    public string Category { get; } = ComputeCategory(trace);

    private static string ComputeCategory(Trace trace)
    {
        var category = trace.Category.Nullify();
        if (category != null)
            return category;

        return category == null ? string.Empty : category;
    }

    internal void WriteHeader(TextWriter writer)
    {
        if (writer == null)
            return;

        writer.WriteCsvCell(nameof(Id));
        writer.WriteCsvCell(nameof(ReceivedUtcDate));
        writer.WriteCsvCell(nameof(Trace.Version));
        writer.WriteCsvCell(nameof(Trace.Level));
        writer.WriteCsvCell(nameof(Trace.Timestamp));
        writer.WriteCsvCell(nameof(Trace.Message), false);
    }

    internal void Write(TextWriter writer)
    {
        if (writer == null)
            return;

        writer.WriteCsvCell(Id.ToString("N"));
        writer.WriteCsvCell(ReceivedUtcDate.ToString("O"));
        writer.WriteCsvCell(Trace.Version.ToString());
        writer.WriteCsvCell(Trace.Level.ToString());
        writer.WriteCsvCell(Trace.Timestamp.ToString("O"));
        writer.WriteCsvCell(Trace.Message, false);
    }
}
