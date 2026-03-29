using System.Text;
using System.Threading;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Writes a sheet to a CSV file
/// </summary>
public interface ICsvWriter
{
    /// <summary>
    /// Writes a sheet to a CSV file
    /// </summary>
    /// <param name="fileName">The path to the CSV file to write</param>
    /// <param name="sheet">The sheet to write</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null, CancellationToken cancellationToken = default);
}