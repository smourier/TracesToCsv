namespace TracesToCsv.Utilities;

public static class CsvUtilities
{
    public static async Task WriteCsvCellAsync(this TextWriter writer, string? cell, bool addSep = true, bool forExcel = true)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // https://support.office.com/en-us/article/excel-specifications-and-limits-1672b34d-7043-467e-8e27-269d656771c3
        // for some reason, it's not 32767
        var max = 32758;
        if (forExcel && cell != null && cell.Length > max)
        {
            cell = cell[..max];
        }

        if (cell != null && cell.IndexOfAny(['\t', '\r', '\n', '"']) >= 0)
        {
            await writer.WriteAsync('"');
            await writer.WriteAsync(cell.Replace("\"", "\"\""));
            await writer.WriteAsync('"');
        }
        else
        {
            await writer.WriteAsync(cell);
        }

        if (addSep)
        {
            await writer.WriteAsync('\t');
        }
    }

    public static void WriteCsvCell(this TextWriter writer, string? cell, bool addSep = true, bool forExcel = true)
    {
        ArgumentNullException.ThrowIfNull(writer);

        // https://support.office.com/en-us/article/excel-specifications-and-limits-1672b34d-7043-467e-8e27-269d656771c3
        // for some reason, it's not 32767
        var max = 32758;
        if (forExcel && cell != null && cell.Length > max)
        {
            cell = cell[..max];
        }

        if (cell != null && cell.IndexOfAny(['\t', '\r', '\n', '"']) >= 0)
        {
            writer.Write('"');
            writer.Write(cell.Replace("\"", "\"\""));
            writer.Write('"');
        }
        else
        {
            writer.Write(cell);
        }

        if (addSep)
        {
            writer.Write('\t');
        }
    }
}
