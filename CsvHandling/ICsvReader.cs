using System.Text;
using System.Threading;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Reads a CSV file and returns a sheet
/// </summary>
public interface ICsvReader
{
    /// <summary>
    /// Reads a CSV file and returns a sheet
    /// </summary>
    /// <param name="fileName">The path to the CSV file to read</param>
    /// <param name="hasRowHeaders">Whether the first row contains column headers</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="strictMode">When true, throws InvalidCsvException if rows have inconsistent column counts (default: false)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null, bool strictMode = false, CancellationToken cancellationToken = default);
}