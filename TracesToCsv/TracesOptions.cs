namespace TracesToCsv;

public class TracesOptions
{
    public const string Traces = "Traces";

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = null!;

    [Range(1000, int.MaxValue)]
    public int FlushTimeout { get; set; } = 1000;

    [Range(1000, int.MaxValue)]
    public int DisposeTimeout { get; set; } = 10000;

    [Required(AllowEmptyStrings = false)]
    public string DirectoryPath { get; set; } = "Traces";

    [Required(AllowEmptyStrings = false)]
    public string FileFormat { get; set; } = "{1}_{0:yyyy}_{0:MM}_{0:dd}.csv";
}
