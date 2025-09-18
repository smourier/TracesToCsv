namespace TracesToCsv.Controllers;

public class TracesOptions
{
    public const string Traces = "Traces";

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = null!;

    [Range(1000, int.MaxValue)]
    public int FlushTimeout { get; set; } = 1000;
}
