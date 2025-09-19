namespace TracesToCsv;

public class TraceFolder : TraceEntry
{
    public required bool IsRoot { get; init; }
    public List<TraceEntry> Entries { get; } = [];

    public override int CompareTo(TraceEntry? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is TraceFile)
            return -1;

        return Name.CompareTo(other.Name);
    }
}
