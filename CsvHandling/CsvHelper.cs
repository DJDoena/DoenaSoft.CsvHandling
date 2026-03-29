using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Helper functions to read/write/create sheets
/// </summary>
public static class CsvHelper
{
    /// <summary>
    /// Reads a CVS file and returns a sheet
    /// </summary>
    /// <param name="fileName">The path to the CSV file to read</param>
    /// <param name="hasRowHeaders">Whether the first row contains column headers</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="strictMode">When true, throws InvalidCsvException if rows have inconsistent column counts (default: false)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    public static ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null, bool strictMode = false, CancellationToken cancellationToken = default)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        var csvSheet = (new CsvReader()).Read(fileName, hasRowHeaders, delimiter, encoding, strictMode, cancellationToken);

        return csvSheet;
    }

    /// <summary>
    /// Writes a sheet to a CSV file
    /// </summary>
    /// <param name="fileName">The path to the CSV file to write</param>
    /// <param name="sheet">The sheet to write</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    public static void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null, CancellationToken cancellationToken = default)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        (new CsvWriter()).Write(fileName, sheet, delimiter, encoding, cancellationToken);
    }

    /// <summary>
    /// Creates a sheet in the given dimensions
    /// </summary>
    public static ICsvSheet CreateSheet(int numberOfColumns, int numberOfRows)
    {
        var sheet = new CsvSheet(false);

        for (var rowNumber = 0; rowNumber < numberOfRows; rowNumber++)
        {
            var row = CreateRow(numberOfColumns);

            sheet.AddRow(row);
        }

        return sheet;
    }

    /// <summary>
    /// Creates a sheet with named headers
    /// </summary>
    public static ICsvSheet CreateSheet(IEnumerable<string> columnNames, int numberOfRows)
    {
        var sheet = new CsvSheet(true);

        sheet.AddHeaderRow(columnNames);

        for (var rowNumber = 0; rowNumber < numberOfRows; rowNumber++)
        {
            var row = CreateRow(columnNames.Count());

            sheet.AddRow(row);
        }

        return sheet;
    }

    /// <summary>
    /// Creates a row with the given number of columns, filled with empty strings
    /// </summary>
    /// <param name="numberOfColumns">number of columns for the row</param>
    /// <returns>an empty row</returns>
    public static List<string> CreateRow(int numberOfColumns)
    {
        var result = new List<string>(numberOfColumns);

        for (var cellIndex = 0; cellIndex < numberOfColumns; cellIndex++)
        {
            result.Add(string.Empty);
        }

        return result;
    }
}