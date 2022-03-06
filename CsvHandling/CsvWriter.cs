using System.IO;
using System.Text;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Writes a sheet to a CSV file
    /// </summary>
    public sealed class CsvWriter : ICsvWriter
    {
        /// <summary>
        /// Writes a sheet to a CSV file
        /// </summary>
        public void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var sw = GetStreamWriter(fs, encoding))
                {
                    foreach (var row in sheet.GetFormattedSheet(delimiter))
                    {
                        sw.WriteLine(row);
                    }
                }
            }
        }

        private static StreamWriter GetStreamWriter(FileStream fs, Encoding encoding) => encoding == null
            ? new StreamWriter(fs)
            : new StreamWriter(fs, encoding);
    }
}
