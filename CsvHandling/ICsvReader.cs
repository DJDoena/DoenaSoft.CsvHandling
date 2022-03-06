using System.Text;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Reads a CVS file and returns a sheet
    /// </summary>
    public interface ICsvReader
    {
        /// <summary>
        /// Reads a CVS file and returns a sheet
        /// </summary>
        ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null);
    }
}