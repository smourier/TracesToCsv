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
}
