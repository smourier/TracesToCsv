namespace TracesToCsv.Controllers;

public class TracesOptions
{
    public const string Traces = "Traces";

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = null!;
}
