using System.Text;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Writes a sheet to a CSV file
    /// </summary>
    public interface ICsvWriter
    {
        /// <summary>
        /// Writes a sheet to a CSV file
        /// </summary>
        void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null);
    }
}