using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Writes a sheet to a CSV file
/// </summary>
public sealed class CsvWriter : ICsvWriter
{
    /// <summary>
    /// Writes a sheet to a CSV file
    /// </summary>
    /// <param name="fileName">The path to the CSV file to write</param>
    /// <param name="sheet">The sheet to write</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    public void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        if (sheet == null)
        {
            throw new ArgumentNullException(nameof(sheet));
        }

        if (!Delimiter.IsValidDelimiter(delimiter))
        {
            throw new ArgumentException($"{nameof(delimiter)} must be one of the valid delimiters: tab, space, comma, semicolon, pipe, tilde, or colon.");
        }

        if (sheet.ColumnCount == 0)
        {
            throw new ArgumentException("Sheet must have at least one column.", nameof(sheet));
        }

        if (sheet.RowCount == 0 && !sheet.HasHeaderRow)
        {
            throw new ArgumentException("Sheet must have at least one row or a header row.", nameof(sheet));
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var sw = GetStreamWriter(fs, encoding);
        foreach (var row in sheet.GetFormattedSheet(delimiter))
        {
            cancellationToken.ThrowIfCancellationRequested();
            sw.WriteLine(row);
        }
    }

    private static StreamWriter GetStreamWriter(FileStream fs, Encoding encoding) => encoding == null
        ? new StreamWriter(fs)
        : new StreamWriter(fs, encoding);
}
