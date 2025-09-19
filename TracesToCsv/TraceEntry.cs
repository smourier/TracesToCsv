namespace TracesToCsv;

public abstract class TraceEntry : IComparable<TraceEntry>
{
    public required string FullName { get; init; }
    public required string ParentFullName { get; init; }
    public string Name => Path.GetFileName(FullName);
    public string ParentName => Path.GetFileName(ParentFullName);

    public abstract int CompareTo(TraceEntry? other);
}
