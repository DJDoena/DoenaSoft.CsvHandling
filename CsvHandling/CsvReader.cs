using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Reads a CSV file and returns a sheet
/// </summary>
public sealed class CsvReader : ICsvReader
{
    private const int TextBeforeErrorInFile = 5;

    private string _fileContent;

    private CsvSheet _csvSheet;

    private char _delimiter;

    private bool _isHeaderRow;

    private bool _strictMode;

    private int _expectedColumnCount;

    private CancellationToken _cancellationToken;

    /// <summary>
    /// Reads a CSV file and returns a sheet
    /// </summary>
    /// <param name="fileName">The path to the CSV file to read</param>
    /// <param name="hasRowHeaders">Whether the first row contains column headers</param>
    /// <param name="delimiter">The delimiter character (default: comma)</param>
    /// <param name="encoding">The text encoding (default: UTF-8)</param>
    /// <param name="strictMode">When true, throws InvalidCsvException if rows have inconsistent column counts (default: false)</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
    public ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null, bool strictMode = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }
        else if (!Delimiter.IsValidDelimiter(delimiter))
        {
            throw new ArgumentException($"{nameof(delimiter)} must be one of the valid delimiters: tab, space, comma, semicolon, pipe, tilde, or colon.");
        }

        _cancellationToken = cancellationToken;

        cancellationToken.ThrowIfCancellationRequested();

        var isMemoryPath = fileName.StartsWith("Memory:");

        if (!isMemoryPath && !File.Exists(fileName))
        {
            throw new FileNotFoundException($"File '{fileName}' does not exist.");
        }

        this.ReadFile(fileName, encoding, isMemoryPath);

        _delimiter = delimiter;

        _csvSheet = new CsvSheet(hasRowHeaders);

        _strictMode = strictMode;

        _expectedColumnCount = -1;

        var charIndex = 0;

        _isHeaderRow = hasRowHeaders;

        while (this.IsNotEndOfFile(charIndex))
        {
            cancellationToken.ThrowIfCancellationRequested();

            charIndex = this.CreateNewRow(charIndex);
        }

        return _csvSheet;
    }

    private void ReadFile(string fileName
        , Encoding encoding
        , bool isMemoryPath)
    {
#if IsWindows
            if (isMemoryPath)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                using var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(fileName.Substring("Memory:".Length));

                using var accessor = mmf.CreateViewAccessor();

                var length = accessor.ReadInt32(0);

                var data = new byte[length];

                accessor.ReadArray(4, data, 0, length);

                _cancellationToken.ThrowIfCancellationRequested();

                _fileContent = encoding != null
                    ? encoding.GetString(data)
                    : Encoding.UTF8.GetString(data);
            }
            else
            {
                this.ReadFileFromDrive(fileName, encoding);
            }

#else
        this.ReadFileFromDrive(fileName, encoding);
#endif
    }

    private void ReadFileFromDrive(string fileName, Encoding encoding)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        _fileContent = encoding != null
            ? File.ReadAllText(fileName, encoding)
            : File.ReadAllText(fileName);
    }

    private int CreateNewRow(int charIndex)
    {
        var cells = new List<string>();

        while (this.IsNotEndOfFile(charIndex))
        {
            var isQuotedCell = this.CharacterIsOpeningQuote(charIndex, out var openingQuoteLength);

            if (isQuotedCell)
            {
                if (this.ReadQuotedCell(ref charIndex, cells, openingQuoteLength))
                {
                    return charIndex;
                }
            }
            else
            {
                if (this.ReadStandardCell(ref charIndex, cells))
                {
                    return charIndex;
                }
            }
        }

        return charIndex;
    }

    private bool ReadQuotedCell(ref int charIndex, List<string> cells, int openingQuoteLength)
    {
        var cellContent = new StringBuilder();

        cellContent.Append('"'); //need to stay in here because CsvSheet will remove them on its own

        charIndex += openingQuoteLength;

        var cellCompleted = false;

        while (this.IsNotEndOfFile(charIndex))
        {
            if (this.CharacterIsMaskedQuote(charIndex))
            {
                cellContent.Append('"');
                cellContent.Append('"');

                charIndex += 2;
            }
            else if (this.CharacterIsClosingQuote(charIndex))
            {
                cellContent.Append('"');

                cells.Add(cellContent.ToString());

                if (this.IsEndOfLine(charIndex + 1, out var lineFeedLength))
                {
                    this.CreateNewRow(cells);

                    charIndex += lineFeedLength + 1;

                    return true;
                }
                else
                {
                    charIndex = this.MoveToNextSeparator(charIndex + 1);
                }

                cellCompleted = true;

                break;
            }
            else
            {
                cellContent.Append(_fileContent[charIndex]);

                charIndex++;
            }
        }

        if (!cellCompleted)
        {
            throw new InvalidCsvException("Last cell in sheet does not close properly.", this.GetLineNumber(charIndex), charIndex);
        }

        if (this.IsEndOfFile(charIndex + 1))
        {
            this.CreateNewRow(cells);

            charIndex++;

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ReadStandardCell(ref int charIndex, List<string> cells)
    {
        var cellContent = new StringBuilder();

        while (this.IsNotEndOfFile(charIndex))
        {
            if (_fileContent[charIndex] == _delimiter)
            {
                cells.Add(cellContent.ToString());

                charIndex++;

                if (this.IsEndOfFile(charIndex))
                {
                    cells.Add(string.Empty);

                    this.CreateNewRow(cells);
                }

                break;
            }
            else if (this.IsEndOfLine(charIndex, out var lineFeedLength))
            {
                cells.Add(cellContent.ToString());

                this.CreateNewRow(cells);

                cells.Clear();

                charIndex += lineFeedLength;

                return true;
            }
            else
            {
                cellContent.Append(_fileContent[charIndex]);

                charIndex++;
            }

            if (this.IsEndOfFile(charIndex))
            {
                cells.Add(cellContent.ToString());

                this.CreateNewRow(cells);

                charIndex++;

                return true;
            }
        }

        return false;
    }

    private void CreateNewRow(List<string> cells)
    {
        if (_strictMode)
        {
            if (_expectedColumnCount == -1)
            {
                _expectedColumnCount = cells.Count;
            }
            else if (cells.Count != _expectedColumnCount)
            {
                var currentLineNumber = this.GetLineNumber(_fileContent.Length);
                throw new InvalidCsvException(
                    $"Row has {cells.Count} column(s), but expected {_expectedColumnCount} column(s).",
                    currentLineNumber,
                    0);
            }
        }

        if (_isHeaderRow)
        {
            _csvSheet.AddHeaderRow(cells);

            _isHeaderRow = false;
        }
        else
        {
            _csvSheet.AddRow(cells);
        }
    }

    private int MoveToNextSeparator(int charIndex)
    {
        while (this.IsNotEndOfFile(charIndex))
        {
            if (_fileContent[charIndex] == _delimiter)
            {
                return charIndex + 1;
            }
            else if (_fileContent[charIndex] == ' ') //if space is the delimiter, we've already exited further up
            {
                charIndex++;
            }
            else if (_fileContent[charIndex] == '\t') //if tab is the delimiter, we've already exited further up
            {
                charIndex++;
            }
            else
            {
                if (charIndex > TextBeforeErrorInFile)
                {
                    var previousText = _fileContent.Substring(charIndex - TextBeforeErrorInFile, TextBeforeErrorInFile);

                    throw new InvalidCsvException($"There is text outside of double quotes in a double-quoted cell (after '{previousText}').", this.GetLineNumber(charIndex), charIndex);
                }
                else
                {
                    throw new InvalidCsvException($"There is text outside of double quotes in a double-quoted cell.", this.GetLineNumber(charIndex), charIndex);
                }
            }
        }

        return charIndex;
    }

    private bool IsNotEndOfFile(int charIndex) => !this.IsEndOfFile(charIndex);

    private bool IsEndOfFile(int charIndex) => charIndex >= _fileContent.Length;

    private bool IsEndOfLine(int charIndex, out int lineFeedLength)
    {
        var isLineFeed = this.IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '\n';

        if (isLineFeed)
        {
            lineFeedLength = 1;

            return true;
        }
        else
        {
            var isCarriageReturnLineFeed = this.IsNotEndOfFile(charIndex + 1) && _fileContent[charIndex] == '\r' && _fileContent[charIndex + 1] == '\n';

            if (isCarriageReturnLineFeed)
            {
                lineFeedLength = 2;

                return true;
            }
            else
            {
                var isCarriageReturnOnly = this.IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '\r';

                if (isCarriageReturnOnly)
                {
                    lineFeedLength = 1;

                    return true;
                }
                else
                {
                    lineFeedLength = 0;

                    return false;
                }
            }
        }
    }

    private bool CharacterIsOpeningQuote(int charIndex, out int quoteLength)
    {
        var isStandardOpeningQuote = this.IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '"';

        if (isStandardOpeningQuote)
        {
            quoteLength = 1;

            return true;
        }
        else
        {
            var isSpacedOpeningQuote = _delimiter != ' ' && this.IsNotEndOfFile(charIndex + 1) && _fileContent[charIndex] == ' ' && _fileContent[charIndex + 1] == '"';

            if (isSpacedOpeningQuote)
            {
                quoteLength = 2;

                return true;
            }
            else
            {
                quoteLength = 0;

                return false;
            }
        }
    }

    private bool CharacterIsClosingQuote(int charIndex)
    {
        var couldBeClosingQuote = this.IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '"';

        if (couldBeClosingQuote)
        {
            var isMaskedQuote = this.CharacterIsMaskedQuote(charIndex + 1);

            if (isMaskedQuote)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private bool CharacterIsMaskedQuote(int charIndex)
        => this.IsNotEndOfFile(charIndex)
        && _fileContent[charIndex] == '"'
        && this.IsNotEndOfFile(charIndex + 1)
        && _fileContent[charIndex + 1] == '"';

    private int GetLineNumber(int charIndex)
    {
        if (string.IsNullOrEmpty(_fileContent) || charIndex < 0)
        {
            return 0;
        }

        var lineNumber = 1;
        for (var i = 0; i < charIndex && i < _fileContent.Length; i++)
        {
            if (_fileContent[i] == '\n')
            {
                lineNumber++;
            }
        }

        return lineNumber;
    }
}