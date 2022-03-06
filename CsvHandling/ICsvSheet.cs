using System.Collections.Generic;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Respresents a comma-separated spreadsheet
    /// </summary>
    /// <remarks>
    /// When working with sheets that have header rows, each header name must be unique for identification purposes
    /// </remarks>
    public interface ICsvSheet
    {
        /// <summary>
        /// Access a single cell
        /// </summary>
        string this[int columnIndex, int rowIndex] { get; set; }

        /// <summary>
        /// Access a single cell
        /// </summary>
        string this[string columnName, int rowIndex] { get; set; }

        /// <summary>
        /// Returns the number of columns the sheet has
        /// </summary>
        int ColumnCount { get; }
        
        /// <summary>
        /// Whether or not the sheet has named headers
        /// </summary>
        bool HasHeaderRow { get; }

        /// <summary>
        /// Returns the number of rows the sheet has
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Adds a column
        /// </summary>
        void AddColumn();

        /// <summary>
        /// Adds a named column
        /// </summary>
        void AddColumn(string columnName);

        /// <summary>
        /// Adds the header row with named columns
        /// </summary>
        void AddHeaderRow(IEnumerable<string> row);

        /// <summary>
        /// Adds a row
        /// </summary>
        void AddRow();

        /// <summary>
        /// Adds a row
        /// </summary>
        void AddRow(IDictionary<string, string> namedRowCells);

        /// <summary>
        /// Adds a row
        /// </summary>
        void AddRow(IEnumerable<string> row);
        
        /// <summary>
        /// Returns the named columns
        /// </summary>
        IEnumerable<string> GetColumnNames();

        /// <summary>
        /// Returns the Excel designation of a column index
        /// </summary>
        /// <example>
        /// columnIndex 0:  A
        /// columnIndex 1:  B
        /// columnIndex 25: Z
        /// columnIndex 26: AA
        /// </example>
        string GetColumnNumber(int columnIndex);

        /// <summary>
        /// Returns the cell content in the final format
        /// </summary>
        /// <remarks>
        /// If the cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        string GetFormattedCell(int columnIndex, int rowIndex, char delimiter = ',');

        /// <summary>
        /// Returns the cell content in the final format
        /// </summary>
        /// <remarks>
        /// If the cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        string GetFormattedCell(string columnName, int rowIndex, char delimiter = ',');

        /// <summary>
        /// Returns the header row in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        string GetFormattedHeaderRow(char delimiter = ',');

        /// <summary>
        /// Returns the row in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        string GetFormattedRow(int rowIndex, char delimiter = ',');

        /// <summary>
        /// Returns the sheet in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        IEnumerable<string> GetFormattedSheet(char delimiter = ',');

        /// <summary>
        /// Returns 1-based number of a row
        /// </summary>
        /// <remarks>
        /// If the sheet has a header row, the first data row will have the number 1.
        /// </remarks>
        string GetRowNumber(int rowIndex);

        /// <summary>
        /// Returns the sheet in a two-dimensional array
        /// </summary>
        string[,] GetSheet();

        /// <summary>
        /// Inserts a column at the given index
        /// </summary>
        void InsertColumn(int columnIndex);

        /// <summary>
        /// Inserts a named column at the given index
        /// </summary>
        void InsertColumn(int columnIndex, string columnName);

        /// <summary>
        /// Inserts a row at the given index
        /// </summary>
        void InsertRow(int rowIndex);

        /// <summary>
        /// Removes the column at the given index
        /// </summary>
        void RemoveColumn(int columnIndex);

        /// <summary>
        /// Removes the column with the given name
        /// </summary>
        void RemoveColumn(string columnName);

        /// <summary>
        /// Removes the row at the given index
        /// </summary>
        void RemoveRow(int rowIndex);

        /// <summary>
        /// Renames a named column
        /// </summary>
        void RenameColumn(string oldName, string newName);
    }
}