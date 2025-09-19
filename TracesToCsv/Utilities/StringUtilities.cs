namespace TracesToCsv.Utilities;

public static class StringUtilities
{
    public static bool EqualsIgnoreCase(this string? thisString, string? text, bool trim = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (trim)
        {
            thisString = thisString.Nullify();
            text = text.Nullify();
        }

        if (thisString == null)
            return text == null;

        if (text == null)
            return false;

        if (thisString.Length != text.Length)
            return false;

        return string.Compare(thisString, text, comparison) == 0;
    }

    public static string? Nullify(this string? text)
    {
        if (text == null)
            return null;

        if (string.IsNullOrWhiteSpace(text))
            return null;

        var t = text.Trim();
        return t.Length == 0 ? null : t;
    }
}
