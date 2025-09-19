namespace TracesToCsv.Utilities;

public static class IOUtilities
{
    public static bool IsPathRooted(string path)
    {
        if (path == null)
            return false;

        var length = path.Length;
        if (length < 1 || path[0] != Path.DirectorySeparatorChar && path[0] != Path.AltDirectorySeparatorChar)
            return length >= 2 && path[1] == Path.VolumeSeparatorChar;

        return true;
    }

    public static bool FileCreateDirectory(string path, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        var dir = Path.GetDirectoryName(path);
        if (dir == null || Directory.Exists(dir))
            return false;

        try
        {
            Directory.CreateDirectory(dir);
            return true;
        }
        catch
        {
            if (throwOnError)
                throw;

            return false;
        }
    }

    public static string? NameToValidFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(fileName.Length);
        foreach (var c in fileName)
        {
            if (Array.IndexOf(invalid, c) < 0)
            {
                sb.Append(c);
            }
        }

        var s = sb.ToString();
        if (s.Length >= 255)
        {
            s = s[..254];
        }

        s = s.Nullify();
        if (s == null)
            return null;

        if (s.All(c => c == '.'))
            return null;

        if (_reservedFileNames.Contains(s))
            return null;

        return s.Nullify();
    }

    private static readonly HashSet<string> _reservedFileNames = new(StringComparer.OrdinalIgnoreCase){
        "con",
        "prn",
        "aux",
        "nul",
        "com0",
        "com1",
        "com2",
        "com3",
        "com4",
        "com5",
        "com6",
        "com7",
        "com8",
        "com9",
        "lpt0",
        "lpt1",
        "lpt2",
        "lpt3",
        "lpt4",
        "lpt5",
        "lpt6",
        "lpt7",
        "lpt8",
        "lpt9"
    };
}
