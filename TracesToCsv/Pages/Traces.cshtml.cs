namespace TracesToCsv.Pages;

public class TracesModel(TracesManager manager, ILogger<TracesModel> logger)
    : PageModel, ILoggable<TracesModel>
{
    ILogger<TracesModel>? ILoggable<TracesModel>.Logger { get; } = logger;

    public string Key { get; private set; } = null!;
    public TraceFolder Folder { get; private set; } = null!;

    public ActionResult OnGet(Guid id, string? url = null)
    {
        Key = manager.GetKey(id);
        var entry = manager.GetEntry(id, url ?? string.Empty);
        if (entry == null)
            return NotFound();

        if (entry is TraceFolder folder)
        {
            Folder = folder;
            return Page();
        }

        var file = (TraceFile)entry;
        var fileResult = IOUtilities.WrapSharingViolations(() =>
        {
            var stream = new FileStream(file.FilePath!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return File(stream, "text/csv", file.Name);
        });
        if (fileResult != null)
            return fileResult;

        return NotFound();
    }
}
