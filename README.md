# DoenaSoft.CsvHandling

This package helps to read and write CSV (comma-separated values) according to [RFC 4180](https://datatracker.ietf.org/doc/html/rfc4180).

## Features

- RFC 4180 compliant CSV reading and writing
- Support for custom delimiters (tab, space, comma, semicolon, pipe, tilde, colon)
- Support for header rows with named columns
- Cell-level access by index or column name
- Multiple encoding support
- Programmatic sheet creation and manipulation

## RFC 4180 Compliance

This library implements RFC 4180 with the following behavior:

### Compliant Features
- ✅ Double quotes (`"`) for escaping fields containing delimiters, quotes, or line breaks
- ✅ Escaped quotes within fields using double-double quotes (`""`)
- ✅ CRLF (`\r\n`) and LF (`\n`) line endings
- ✅ Optional header row
- ✅ Last record may or may not have an ending line break

### Extended Features (Beyond RFC 4180)
- ✅ **Custom delimiters:** Supports tab, space, comma, semicolon, pipe, tilde, and colon (RFC 4180 only specifies comma)
- ✅ **Inconsistent row lengths:** Rows with fewer columns are automatically padded with empty cells; rows with more columns automatically extend the sheet
  - *Note:* This is lenient behavior - RFC 4180 expects all records to have the same number of fields
  - Use `strictMode: true` to throw `InvalidCsvException` for inconsistent row lengths
- ✅ **Empty files:** Returns an empty sheet (0 rows, 0 columns)
- ✅ **Multiple encodings:** UTF-8 (default), UTF-16, ASCII, etc.
- ✅ **Line ending support:** CRLF (`\r\n`), LF (`\n`), and CR (`\r`) line endings

### Validation and Error Handling
- ❌ **Unclosed quotes:** Throws `InvalidCsvException` with line/position information
- ❌ **Text outside quotes:** Throws `InvalidCsvException` when non-whitespace text appears after closing quote
- ❌ **Invalid delimiters:** Throws `ArgumentException` when delimiter is not in the valid list
- ❌ **Empty sheets:** `CsvWriter` throws `ArgumentException` when attempting to write sheets with 0 columns or 0 rows (unless it has a header row)
- ❌ **Duplicate column names:** `CsvSheet.AddHeaderRow` throws `ArgumentException` if column names are not unique (case-insensitive)
- ❌ **Inconsistent rows (strict mode):** When `strictMode: true`, throws `InvalidCsvException` if rows have different column counts

### Encoding Behavior
- **Default Encoding:** UTF-8 is used when no encoding is specified
- **BOM Handling:** Byte Order Mark (BOM) is automatically detected and handled by .NET's `File.ReadAllText` when no explicit encoding is provided
  - UTF-8 with BOM: Automatically detected
  - UTF-16 LE/BE with BOM: Automatically detected
  - To explicitly handle BOM, provide the appropriate `Encoding` instance (e.g., `new UTF8Encoding(true)` for UTF-8 with BOM)
- **Writing:** BOM is included when writing if the `Encoding` instance is configured to emit it (e.g., `new UTF8Encoding(true)`)
- **Invalid Sequences:** Invalid byte sequences for the specified encoding will result in replacement characters (�) or throw exceptions depending on the encoding's error handling configuration

### Known Limitations
- Does not support custom quote characters (only `"` is supported per RFC 4180)
- Entire file is loaded into memory (not suitable for multi-GB files - consider splitting large files)
- No progress reporting for long operations (use `CancellationToken` to cancel if needed)

## Public API

### Classes

#### `CsvReader`
Reads a CSV file and returns a sheet representation.

**Methods:**
- `ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null, bool strictMode = false, CancellationToken cancellationToken = default)` - Reads a CSV file and returns an ICsvSheet
  - When `strictMode` is `true`, throws `InvalidCsvException` if rows have inconsistent column counts
  - Supports cancellation via `CancellationToken`

#### `CsvWriter`
Writes a sheet to a CSV file.

**Methods:**
- `void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null, CancellationToken cancellationToken = default)` - Writes an ICsvSheet to a CSV file
  - Validates that the sheet has at least one column and (one row or a header row)
  - Supports cancellation via `CancellationToken`

#### `CsvSheet`
Represents a comma-separated spreadsheet with support for named headers and cell manipulation.

**Properties:**
- `int ColumnCount` - Returns the number of columns
- `int RowCount` - Returns the number of rows
- `bool HasHeaderRow` - Whether or not the sheet has named headers
- `string this[int columnIndex, int rowIndex]` - Access a single cell by indices
- `string this[string columnName, int rowIndex]` - Access a single cell by column name

**Key Methods:**
- `AddColumn()` / `AddColumn(string columnName)` - Adds a column
- `AddRow()` / `AddRow(IEnumerable<string> row)` / `AddRow(IDictionary<string, string> namedRowCells)` - Adds a row
- `AddHeaderRow(IEnumerable<string> row)` - Adds the header row with named columns
- `InsertColumn(int columnIndex)` / `InsertColumn(int columnIndex, string columnName)` - Inserts a column
- `InsertRow(int rowIndex)` - Inserts a row
- `RemoveColumn(int columnIndex)` / `RemoveColumn(string columnName)` - Removes a column
- `RemoveRow(int rowIndex)` - Removes a row
- `RenameColumn(string oldName, string newName)` - Renames a named column
- `GetColumnNames()` - Returns the named columns
- `GetFormattedSheet(char delimiter = ',')` - Returns the sheet in final format
- `GetSheet()` - Returns the sheet as a two-dimensional array

#### `CsvHelper`
Static helper class for convenient CSV operations.

**Methods:**
- `ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null, bool strictMode = false, CancellationToken cancellationToken = default)` - Reads a CSV file
  - When `strictMode` is `true`, throws `InvalidCsvException` if rows have inconsistent column counts
  - Supports cancellation via `CancellationToken`
- `void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null, CancellationToken cancellationToken = default)` - Writes a sheet to CSV
  - Supports cancellation via `CancellationToken`
- `ICsvSheet CreateSheet(int numberOfColumns, int numberOfRows)` - Creates an empty sheet
- `ICsvSheet CreateSheet(IEnumerable<string> columnNames, int numberOfRows)` - Creates a sheet with named headers

#### `Delimiter`
Static class that provides valid field delimiters.

**Methods:**
- `IEnumerable<char> GetValidDelimiters()` - Returns valid delimiters: tab, space, comma, semicolon, pipe, tilde, colon

#### `InvalidCsvException`
Exception thrown when invalid CSV format is encountered.

### Interfaces

#### `ICsvReader`
Interface for reading CSV files.

**Methods:**
- `ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null)`

#### `ICsvWriter`
Interface for writing CSV files.

**Methods:**
- `void Write(string fileName, ICsvSheet sheet, char delimiter = ',', Encoding encoding = null)`

#### `ICsvSheet`
Interface representing a comma-separated spreadsheet.

**Note:** When working with sheets that have header rows, each header name must be unique for identification purposes.

## Usage Examples

### Reading a CSV file
```csharp
using DoenaSoft.CsvHandling;

// Read a CSV file with headers
var sheet = CsvHelper.Read("data.csv", hasRowHeaders: true, delimiter: ',');

// Access cell by column name
string value = sheet["ColumnName", 0];

// Access cell by index
string value2 = sheet[0, 0];
```

### Writing a CSV file
```csharp
using DoenaSoft.CsvHandling;

// Create a sheet with headers
var sheet = CsvHelper.CreateSheet(new[] { "Name", "Age", "City" }, numberOfRows: 0);

// Add rows
sheet.AddRow(new Dictionary<string, string> 
{
    { "Name", "John" },
    { "Age", "30" },
    { "City", "New York" }
});

// Write to file
CsvHelper.Write("output.csv", sheet, delimiter: ',');
```

### Creating a sheet programmatically
```csharp
using DoenaSoft.CsvHandling;

// Create empty sheet
var sheet = new CsvSheet(hasHeaderRow: true);

// Add columns
sheet.AddColumn("FirstName");
sheet.AddColumn("LastName");
sheet.AddColumn("Email");

// Add data rows
sheet.AddRow(new[] { "Jane", "Doe", "jane@example.com" });
sheet.AddRow(new[] { "Bob", "Smith", "bob@example.com" });

// Manipulate data
sheet["Email", 0] = "jane.doe@example.com";

// Output
CsvHelper.Write("contacts.csv", sheet);
```

### Using Strict Mode for Data Validation
```csharp
using DoenaSoft.CsvHandling;

try
{
    // Strict mode ensures all rows have the same number of columns
    var sheet = CsvHelper.Read("data.csv", hasRowHeaders: true, strictMode: true);
    
    // If we reach here, all rows are consistent
    Console.WriteLine($"Valid CSV with {sheet.RowCount} rows");
}
catch (InvalidCsvException ex)
{
    // Catch inconsistent row lengths
    Console.WriteLine($"Invalid CSV: {ex.Message}");
    Console.WriteLine($"Error at line {ex.LineNumber}, position {ex.CharPosition}");
}
```

### Handling Different Encodings and BOM
```csharp
using System.Text;
using DoenaSoft.CsvHandling;

// Read UTF-8 file with BOM (auto-detected)
var sheet1 = CsvHelper.Read("utf8-bom.csv", hasRowHeaders: true);

// Read UTF-16 file explicitly
var sheet2 = CsvHelper.Read("utf16.csv", hasRowHeaders: true, encoding: Encoding.Unicode);

// Write with UTF-8 BOM
CsvHelper.Write("output-bom.csv", sheet1, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
```

### Using CancellationToken for Long Operations
```csharp
using System.Threading;
using DoenaSoft.CsvHandling;

// Create a cancellation token source
using var cts = new CancellationTokenSource();

// Set a timeout or allow user to cancel
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    // Read large file with cancellation support
    var sheet = CsvHelper.Read("large-file.csv", hasRowHeaders: true, cancellationToken: cts.Token);
    
    // Process the data
    Console.WriteLine($"Loaded {sheet.RowCount} rows");
    
    // Write with cancellation support
    CsvHelper.Write("output.csv", sheet, cancellationToken: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```