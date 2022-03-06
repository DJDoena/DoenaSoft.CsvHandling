using System;
using System.Collections.Generic;
using System.Linq;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Respresents a comma-separated spreadsheet
    /// </summary>
    /// <remarks>
    /// When working with sheets that have header rows, each header name must be unique for identification purposes
    /// </remarks>
    public sealed class CsvSheet : ICsvSheet
    {
        //Key: ColumnIndex, Value: ColumnCells (vertical)
        private readonly Dictionary<int, List<string>> _columns;

        private readonly Dictionary<int, string> _columnNames;

        /// <summary />
        public CsvSheet(bool hasHeaderRow)
        {
            _columns = new Dictionary<int, List<string>>();

            if (hasHeaderRow)
            {
                _columnNames = new Dictionary<int, string>();
            }
            else
            {
                _columnNames = null;
            }
        }

        /// <summary>
        /// Returns the number of columns the sheet has
        /// </summary>
        public int ColumnCount => _columns.Count;

        /// <summary>
        /// Returns the number of rows the sheet has
        /// </summary>
        public int RowCount => _columns.Any()
            ? _columns.Values.First().Count
            : 0;

        /// <summary>
        /// Whether or not the sheet has named headers
        /// </summary>
        public bool HasHeaderRow => _columnNames != null;

        /// <summary>
        /// Access a single cell
        /// </summary>
        public string this[int columnIndex, int rowIndex]
        {
            get
            {
                CheckIndex(rowIndex, nameof(rowIndex));
                CheckIndex(columnIndex, nameof(columnIndex));

                if (columnIndex >= _columns.Count)
                {
                    throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range.");
                }

                var cells = _columns[columnIndex];

                if (rowIndex >= cells.Count)
                {
                    throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range.");
                }

                return cells[rowIndex];
            }
            set
            {
                CheckIndex(rowIndex, nameof(rowIndex));
                CheckIndex(columnIndex, nameof(columnIndex));

                if (columnIndex >= _columns.Count)
                {
                    throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range.");
                }

                var cells = _columns[columnIndex];

                if (rowIndex >= cells.Count)
                {
                    throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range.");
                }

                cells[rowIndex] = MakeCleanCell(value);
            }
        }

        /// <summary>
        /// Access a single cell
        /// </summary>
        public string this[string columnName, int rowIndex]
        {
            get
            {
                CheckIndex(rowIndex, nameof(rowIndex));

                if (!this.HasHeaderRow)
                {
                    throw new InvalidOperationException("Sheet has no header row.");
                }
                else if (string.IsNullOrWhiteSpace(columnName))
                {
                    throw new ArgumentNullException(nameof(columnName));
                }

                if (!this.TryGetColumnIndex(columnName, out var columnIndex))
                {
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                }

                return this[columnIndex, rowIndex];
            }
            set
            {
                CheckIndex(rowIndex, nameof(rowIndex));

                if (!this.HasHeaderRow)
                {
                    throw new InvalidOperationException("Sheet has no header row.");
                }
                else if (string.IsNullOrWhiteSpace(columnName))
                {
                    throw new ArgumentNullException(nameof(columnName));
                }

                if (!this.TryGetColumnIndex(columnName, out var columnIndex))
                {
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                }

                this[columnIndex, rowIndex] = value;
            }
        }

        /// <summary>
        /// Adds a column
        /// </summary>
        public void AddColumn() => this.AddNewColumn(this.ColumnCount, "Undefined", true);

        /// <summary>
        /// Adds a named column
        /// </summary>
        public void AddColumn(string columnName)
        {
            if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }
            else if (this.TryGetColumnIndex(columnName, out _))
            {
                throw new ArgumentException($"Column '{columnName}' already exists.");
            }

            this.AddNewColumn(this.ColumnCount, columnName, false);
        }

        /// <summary>
        /// Inserts a column at the given index
        /// </summary>
        public void InsertColumn(int columnIndex) => this.InsertColumn(columnIndex, "Undefined", true);

        /// <summary>
        /// Inserts a named column at the given index
        /// </summary>
        public void InsertColumn(int columnIndex, string columnName)
        {
            if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }
            else if (this.TryGetColumnIndex(columnName, out _))
            {
                throw new ArgumentException($"Column '{columnName}' already exists.");
            }

            this.InsertColumn(columnIndex, columnName, false);
        }

        /// <summary>
        /// Returns the named columns
        /// </summary>
        public IEnumerable<string> GetColumnNames()
        {
            if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }

            var orderedColumns = _columnNames.OrderBy(kvp => kvp.Key);

            foreach (var column in orderedColumns)
            {
                yield return column.Value;
            }
        }

        /// <summary>
        /// Adds the header row with named columns
        /// </summary>
        public void AddHeaderRow(IEnumerable<string> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }
            else if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (_columnNames.Count > 0)
            {
                throw new InvalidOperationException("Header row was already initialized.");
            }
            else if (row.Count() == int.MaxValue)
            {
                throw new ArgumentException("Column count exceeds limit.");
            }

            for (var columnIndex = 0; columnIndex < row.Count(); columnIndex++)
            {
                var columnName = row.ElementAt(columnIndex);

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    throw new ArgumentException("Column name must not be empty.");
                }

                _columns.Add(columnIndex, new List<string>());

                this.AddNamedColumn(columnIndex, columnName, false);
            }
        }

        /// <summary>
        /// Adds a row
        /// </summary>
        public void AddRow()
        {
            if (this.RowCount == int.MaxValue)
            {
                throw new ArgumentException("Row count exceeds limit.");
            }
            else if (this.ColumnCount == 0)
            {
                throw new InvalidOperationException("Sheet does not have columns yet.");
            }

            this.AddRow(new List<string>() { string.Empty });
        }

        /// <summary>
        /// Adds a row
        /// </summary>
        public void AddRow(IEnumerable<string> row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }
            else if (this.RowCount == int.MaxValue)
            {
                throw new ArgumentException("Row count exceeds limit.");
            }
            else if (row.Count() == int.MaxValue)
            {
                throw new ArgumentException("Column count exceeds limit.");
            }

            var previousRowCount = this.RowCount;

            for (var columnIndex = 0; columnIndex < row.Count(); columnIndex++)
            {
                var cell = row.ElementAt(columnIndex);

                cell = MakeCleanCell(cell);

                if (!_columns.TryGetValue(columnIndex, out var cells))
                {
                    this.AddNewColumn(columnIndex, "Undefined", true);

                    cells = _columns[columnIndex];
                }

                if (cells.Count == previousRowCount)
                {
                    cells.Add(cell);
                }
                else
                {
                    cells[previousRowCount] = cell;
                }
            }

            this.FillUpMissingCells();
        }

        /// <summary>
        /// Adds a row
        /// </summary>
        public void AddRow(IDictionary<string, string> namedRowCells)
        {
            if (namedRowCells == null)
            {
                throw new ArgumentNullException(nameof(namedRowCells));
            }
            else if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (namedRowCells.Keys.Any(key => !this.TryGetColumnIndex(key, out _)))
            {
                var missingColumn = namedRowCells.Keys.First(key => !this.TryGetColumnIndex(key, out _));

                throw new ArgumentException($"Column '{missingColumn}' does not exist.");
            }

            this.AddRow();

            foreach (var cell in namedRowCells)
            {
                this[cell.Key, this.RowCount - 1] = cell.Value;
            }
        }

        /// <summary>
        /// Inserts a row at the given index
        /// </summary>
        public void InsertRow(int rowIndex)
        {
            CheckIndex(rowIndex, nameof(rowIndex));

            if (rowIndex >= this.RowCount)
            {
                throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range.");
            }

            foreach (var cells in _columns.Values)
            {
                cells.Insert(rowIndex, string.Empty);
            }
        }

        /// <summary>
        /// Returns the sheet in a two-dimensional array
        /// </summary>
        public string[,] GetSheet()
        {
            var rowCount = this.HasHeaderRow
                ? this.RowCount + 1
                : this.RowCount;

            var sheet = new string[this.ColumnCount, rowCount];

            for (var columnIndex = 0; columnIndex < this.ColumnCount; columnIndex++)
            {
                var rowOffset = 0;

                if (this.HasHeaderRow)
                {
                    sheet[columnIndex, 0] = _columnNames[columnIndex];

                    rowOffset++;
                }

                for (var rowIndex = 0; rowIndex < this.RowCount; rowIndex++)
                {
                    sheet[columnIndex, rowIndex + rowOffset] = this[columnIndex, rowIndex];
                }
            }

            return sheet;
        }

        /// <summary>
        /// Returns the sheet in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        public IEnumerable<string> GetFormattedSheet(char delimiter = ',')
        {
            if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character.");
            }

            if (this.HasHeaderRow)
            {
                var headerRow = this.GetFormattedHeaderRow(delimiter);

                yield return headerRow;
            }

            for (var rowIndex = 0; rowIndex < this.RowCount; rowIndex++)
            {
                var row = this.GetFormattedRow(rowIndex, delimiter);

                yield return row;
            }
        }

        /// <summary>
        /// Returns the header row in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        public string GetFormattedHeaderRow(char delimiter = ',')
        {
            if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character.");
            }
            else if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }

            var orderedColumns = _columnNames.OrderBy(kvp => kvp.Key);

            var formattedCells = new List<string>(this.ColumnCount);

            foreach (var column in orderedColumns)
            {
                formattedCells.Add(GetFormattedCell(column.Value, delimiter));
            }

            var row = string.Join(delimiter.ToString(), formattedCells);

            return row;
        }

        /// <summary>
        /// Returns the row in the final format
        /// </summary>
        /// <remarks>
        /// If a cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        public string GetFormattedRow(int rowIndex, char delimiter = ',')
        {
            if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character.");
            }

            var formattedCells = new List<string>(this.ColumnCount);

            for (var colIndex = 0; colIndex < this.ColumnCount; colIndex++)
            {
                formattedCells.Add(this.GetFormattedCell(colIndex, rowIndex, delimiter));
            }

            var row = string.Join(delimiter.ToString(), formattedCells);

            return row;
        }

        /// <summary>
        /// Returns the cell content in the final format
        /// </summary>
        /// <remarks>
        /// If the cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        public string GetFormattedCell(int columnIndex, int rowIndex, char delimiter = ',')
        {
            if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character.");
            }

            var cell = this[columnIndex, rowIndex];

            cell = GetFormattedCell(cell, delimiter);

            return cell;
        }

        /// <summary>
        /// Returns the cell content in the final format
        /// </summary>
        /// <remarks>
        /// If the cell contains the " character, a newline character or the delimiter character, the content will be enclosed by " characters.
        /// </remarks>
        public string GetFormattedCell(string columnName, int rowIndex, char delimiter = ',')
        {
            if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character.");
            }

            var cell = this[columnName, rowIndex];

            cell = GetFormattedCell(cell, delimiter);

            return cell;
        }

        /// <summary>
        /// Renames a named column
        /// </summary>
        public void RenameColumn(string oldName, string newName)
        {
            if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (string.IsNullOrWhiteSpace(oldName))
            {
                throw new ArgumentNullException(nameof(oldName));
            }
            else if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (!this.TryGetColumnIndex(oldName, out var columnIndex))
            {
                throw new ArgumentException($"Column '{oldName}' does not exist.");
            }
            else if (this.TryGetColumnIndex(newName, out _))
            {
                throw new ArgumentException($"Column '{newName}' already exists.");
            }

            _columnNames[columnIndex] = newName;
        }

        /// <summary>
        /// Removes the column at the given index
        /// </summary>
        public void RemoveColumn(int columnIndex)
        {
            CheckIndex(columnIndex, nameof(columnIndex));

            if (columnIndex >= this.ColumnCount)
            {
                throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range.");
            }

            for (var currentColumnIndex = 0; currentColumnIndex < _columns.Count; currentColumnIndex++)
            {
                if (currentColumnIndex < columnIndex)
                {
                    continue;
                }
                else if (currentColumnIndex == columnIndex)
                {
                    _columns.Remove(currentColumnIndex);

                    if (this.HasHeaderRow)
                    {
                        _columnNames.Remove(currentColumnIndex);
                    }
                }
                else
                {
                    var newColumnIndex = currentColumnIndex - 1;

                    var cells = _columns[currentColumnIndex];

                    _columns.Add(newColumnIndex, cells);
                    _columns.Remove(currentColumnIndex);

                    if (this.HasHeaderRow)
                    {
                        var columnName = _columnNames[currentColumnIndex];

                        _columnNames.Add(newColumnIndex, columnName);
                        _columnNames.Remove(currentColumnIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the column with the given name
        /// </summary>
        public void RemoveColumn(string columnName)
        {
            if (!this.HasHeaderRow)
            {
                throw new InvalidOperationException("Sheet has no header row.");
            }
            else if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (!this.TryGetColumnIndex(columnName, out var columnIndex))
            {
                throw new ArgumentException($"Column '{columnName}' does not exist.");
            }

            this.RemoveColumn(columnIndex);
        }

        /// <summary>
        /// Removes the row at the given index
        /// </summary>
        public void RemoveRow(int rowIndex)
        {
            CheckIndex(rowIndex, nameof(rowIndex));

            if (rowIndex >= this.RowCount)
            {
                throw new IndexOutOfRangeException($"Row index {rowIndex} is out of range.");
            }

            foreach (var cells in _columns.Values)
            {
                cells.RemoveAt(rowIndex);
            }
        }

        /// <summary>
        /// Returns 1-based number of a row
        /// </summary>
        /// <remarks>
        /// If the sheet has a header row, the first data row will have the number 1.
        /// </remarks>
        public string GetRowNumber(int rowIndex)
        {
            CheckIndex(rowIndex, nameof(rowIndex));

            var rowNumber = this.HasHeaderRow
                ? rowIndex + 2
                : rowIndex + 1;

            return rowNumber.ToString();
        }

        /// <summary>
        /// Returns the Excel designation of a column index
        /// </summary>
        /// <example>
        /// columnIndex 0:  A
        /// columnIndex 1:  B
        /// columnIndex 25: Z
        /// columnIndex 26: AA
        /// </example>
        public string GetColumnNumber(int columnIndex)
        {
            CheckIndex(columnIndex, nameof(columnIndex));

            const int MaxBuffer = 32;

            var columnNames = Enumerable.Range('A', 26).Select(x => (char)x).ToArray();

            int bufferIndex = MaxBuffer;

            var buffer = new char[bufferIndex];

            var targetBase = columnNames.Length;

            do
            {
                --columnIndex;

                var columnNameIndex = columnIndex % targetBase;

                --bufferIndex;

                buffer[bufferIndex] = columnNames[columnNameIndex];

                columnIndex /= targetBase;
            }
            while (columnIndex > 0);

            var result = new char[MaxBuffer - bufferIndex];

            Array.Copy(buffer, bufferIndex, result, 0, MaxBuffer - bufferIndex);

            return new string(result);
        }

        private static string GetFormattedCell(string cell, char delimiter)
        {
            if (cell.Contains('"'))
            {
                cell = cell.Replace("\"", "\"\"");
            }

            if (cell.Contains('"') || cell.Contains('\n') || cell.Contains(delimiter))
            {
                cell = "\"" + cell + "\"";
            }

            return cell;
        }

        private static string MakeCleanCell(string cell)
        {
            if (cell == null)
            {
                cell = string.Empty;
            }

            if (cell.Length >= 2 && cell.First() == '"' && cell.Last() == '"')
            {
                cell = cell.Substring(1, cell.Length - 2);

                cell = cell.Replace("\"\"", "\"");
            }

            return cell;
        }

        private void AddNewColumn(int columnIndex, string columnName, bool canRename)
        {
            _columns.Add(columnIndex, new List<string>());

            if (this.HasHeaderRow)
            {
                this.AddNamedColumn(columnIndex, columnName, canRename);
            }

            this.FillUpMissingCells();
        }

        private void FillUpMissingCells()
        {
            var maxRows = _columns.Max(kvp => kvp.Value.Count);

            foreach (var cells in _columns.Values.Where(c => c.Count < maxRows))
            {
                while (cells.Count < maxRows)
                {
                    cells.Add(string.Empty);
                }
            }
        }

        private void InsertColumn(int columnIndex, string columnName, bool canBeRenamed)
        {
            CheckIndex(columnIndex, nameof(columnIndex));

            if (columnIndex >= this.ColumnCount)
            {
                throw new IndexOutOfRangeException($"Column index {columnIndex} is out of range.");
            }

            for (var currentColumnIndex = this.ColumnCount - 1; currentColumnIndex >= 0; currentColumnIndex--)
            {
                if (columnIndex > currentColumnIndex)
                {
                    var newColumnIndex = currentColumnIndex + 1;

                    var cells = _columns[currentColumnIndex];

                    _columns.Add(newColumnIndex, cells);
                    _columns.Remove(currentColumnIndex);

                    if (this.HasHeaderRow)
                    {
                        var currentColumnName = _columnNames[currentColumnIndex];

                        _columnNames.Add(newColumnIndex, currentColumnName);
                        _columnNames.Remove(currentColumnIndex);
                    }
                }
                else if (columnIndex == currentColumnIndex)
                {
                    this.AddNewColumn(columnIndex, columnName, canBeRenamed);
                }
                else
                {
                    break;
                }
            }
        }

        private void AddNamedColumn(int columnIndex, string columnName, bool canRename)
        {
            if (!this.TryGetColumnIndex(columnName, out _))
            {
                _columnNames.Add(columnIndex, columnName);
            }
            else if (canRename)
            {
                for (var suffixIndex = 1; suffixIndex < int.MaxValue; suffixIndex++)
                {
                    var newName = $"{columnName}_{suffixIndex}";

                    if (!this.TryGetColumnIndex(newName, out _))
                    {
                        _columnNames.Add(columnIndex, newName);

                        return;
                    }
                }

                throw new IndexOutOfRangeException("Too many columns.");
            }
            else
            {
                throw new ArgumentException($"Column name '{columnName}' is already in use.");
            }
        }

        private static void CheckIndex(int index, string indexName)
        {
            if (index < 0)
            {
                throw new ArgumentException($"{indexName} must not be less than 0.");
            }
        }

        private bool TryGetColumnIndex(string columnName, out int columnIndex)
        {
            var columnInfo = _columnNames.FirstOrDefault(kvp => kvp.Value == columnName);

            if (columnInfo.Value == columnName)
            {
                columnIndex = columnInfo.Key;

                return true;
            }
            else
            {
                columnIndex = -1;

                return false;
            }
        }
    }
}