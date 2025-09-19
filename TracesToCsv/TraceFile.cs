namespace TracesToCsv;

public class TraceFile : TraceEntry
{
    public required long Length { get; init; }
    public string? FilePath { get; set; }
    public required DateTimeOffset LastWriteTimeUtc { get; init; }

    public override int CompareTo(TraceEntry? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is TraceFolder)
            return 1;

        return Name.CompareTo(other.Name);
    }
}
