using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Helper functions to read/write/create sheets
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// Reads a CVS file and returns a sheet
        /// </summary>
        public static ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var csvSheet = (new CsvReader()).Read(fileName, hasRowHeaders, delimiter, encoding);

            return csvSheet;
        }

        /// <summary>
        /// Writes a sheet to a CSV file
        /// </summary>
        public static void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            (new CsvWriter()).Write(fileName, sheet, delimiter, encoding);
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

        private static IEnumerable<string> CreateRow(int numberOfColumns)
        {
            var result = new List<string>(numberOfColumns);

            for (var cellIndex = 0; cellIndex < numberOfColumns; cellIndex++)
            {
                result.Add(string.Empty);
            }

            return result;
        }
    }
}