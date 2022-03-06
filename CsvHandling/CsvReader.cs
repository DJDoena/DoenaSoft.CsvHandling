using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoenaSoft.CsvHandling
{
    /// <summary>
    /// Reads a CVS file and returns a sheet
    /// </summary>
    public sealed class CsvReader : ICsvReader
    {
        private string _fileContent;

        private CsvSheet _csvSheet;

        private char _delimiter;

        private bool _isHeaderRow;

        /// <summary>
        /// Reads a CVS file and returns a sheet
        /// </summary>
        public ICsvSheet Read(string fileName, bool hasRowHeaders, char delimiter = ',', Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(fileName);
            }
            else if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"File '{fileName}' does not exist.");
            }
            else if (!Delimiter.GetValidDelimiters().Contains(delimiter))
            {
                throw new ArgumentException($"{nameof(delimiter)} must not be a control character!");
            }

            _fileContent = encoding != null
                ? File.ReadAllText(fileName, encoding)
                : File.ReadAllText(fileName);

            _delimiter = delimiter;

            _csvSheet = new CsvSheet(hasRowHeaders);

            var charIndex = 0;

            _isHeaderRow = hasRowHeaders;

            while (IsNotEndOfFile(charIndex))
            {
                charIndex = CreateNewRow(charIndex);
            }

            return _csvSheet;
        }

        private int CreateNewRow(int charIndex)
        {
            var cells = new List<string>();

            while (IsNotEndOfFile(charIndex))
            {
                var isQuotedCell = CharacterIsOpeningQuote(charIndex, out var openingQuoteLength);

                if (isQuotedCell)
                {
                    if (ReadQuotedCell(ref charIndex, cells, openingQuoteLength))
                    {
                        return charIndex;
                    }
                }
                else
                {
                    if (ReadStandardCell(ref charIndex, cells))
                    {
                        return charIndex;
                    }
                }
            }

            return charIndex;
        }

        private bool ReadQuotedCell(ref int charIndex, List<string> cells, int openingQuoteLength)
        {
            var cellContent = new List<char>();

            cellContent.Add('"'); //need to stay in here because CsvSheet will remove them on its own

            charIndex += openingQuoteLength;

            var cellCompleted = false;

            while (IsNotEndOfFile(charIndex))
            {
                if (CharacterIsMaskedQuote(charIndex))
                {
                    cellContent.Add('"');
                    cellContent.Add('"');

                    charIndex += 2;
                }
                else if (CharacterIsClosingQuote(charIndex))
                {
                    cellContent.Add('"');

                    cells.Add(new string(cellContent.ToArray()));

                    if (IsEndOfLine(charIndex + 1, out var lineFeedLength))
                    {
                        CreateNewRow(cells);

                        charIndex += lineFeedLength + 1;

                        return true;
                    }
                    else
                    {
                        charIndex = MoveToNextSeparator(charIndex + 1);
                    }

                    cellCompleted = true;

                    break;
                }
                else
                {
                    cellContent.Add(_fileContent[charIndex]);

                    charIndex++;
                }
            }

            if (!cellCompleted)
            {
                throw new InvalidCsvException("Last cell in sheet does not close properly.");
            }

            if (IsEndOfFile(charIndex + 1))
            {
                CreateNewRow(cells);

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
            var cellContent = new List<char>();

            while (IsNotEndOfFile(charIndex))
            {
                if (_fileContent[charIndex] == _delimiter)
                {
                    cells.Add(new string(cellContent.ToArray()));

                    charIndex++;

                    break;
                }
                else if (IsEndOfLine(charIndex, out var lineFeedLength))
                {
                    cells.Add(new string(cellContent.ToArray()));

                    CreateNewRow(cells);

                    cells.Clear();

                    charIndex += lineFeedLength;

                    return true;
                }
                else
                {
                    cellContent.Add(_fileContent[charIndex]);

                    charIndex++;
                }

                if (IsEndOfFile(charIndex))
                {
                    cells.Add(new string(cellContent.ToArray()));

                    CreateNewRow(cells);

                    charIndex++;

                    return true;
                }
            }

            return false;
        }

        private void CreateNewRow(List<string> cells)
        {
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
            while (IsNotEndOfFile(charIndex))
            {
                if (_fileContent[charIndex] == _delimiter)
                {
                    return charIndex + 1;
                }
                else if (_fileContent[charIndex] == ' ') //if space is the delimiter, we've already exitedexited further up
                {
                    charIndex++;
                }
                else if (_fileContent[charIndex] == '\t') //if tab is the delimiter, we've already exitedexited further up
                {
                    charIndex++;
                }
                else
                {
                    if (charIndex > 5)
                    {
                        var previousText = _fileContent.Substring(charIndex - 5, 5);

                        throw new InvalidCsvException($"There is text outside of double quotes in a double-quoted cell (after '{previousText}' at index: {charIndex}).");
                    }
                    else
                    {
                        throw new InvalidCsvException($"There is text outside of double quotes in a double-quoted cell (index: {charIndex}).");
                    }
                }
            }

            return charIndex;
        }

        private bool IsNotEndOfFile(int charIndex) => !IsEndOfFile(charIndex);

        private bool IsEndOfFile(int charIndex) => charIndex >= _fileContent.Length;

        private bool IsEndOfLine(int charIndex, out int lineFeedLength)
        {
            var isLineFeed = IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '\n';

            if (isLineFeed)
            {
                lineFeedLength = 1;

                return true;
            }
            else
            {
                var isCarriageReturnLineFeed = IsNotEndOfFile(charIndex + 1) && _fileContent[charIndex] == '\r' && _fileContent[charIndex + 1] == '\n';

                if (isCarriageReturnLineFeed)
                {
                    lineFeedLength = 2;

                    return true;
                }
                else
                {
                    lineFeedLength = 0;

                    return false;
                }
            }
        }

        private bool CharacterIsOpeningQuote(int charIndex, out int quoteLength)
        {
            var isStandardOpeningQuote = IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '"';

            if (isStandardOpeningQuote)
            {
                quoteLength = 1;

                return true;
            }
            else
            {
                var isSpacedOpeningQuote = _delimiter != ' ' && IsNotEndOfFile(charIndex + 1) && _fileContent[charIndex] == ' ' && _fileContent[charIndex + 1] == '"';

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
            var couldBeClosingQuote = IsNotEndOfFile(charIndex) && _fileContent[charIndex] == '"';

            if (couldBeClosingQuote)
            {
                var isMaskedQuote = CharacterIsMaskedQuote(charIndex + 1);

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

        private bool CharacterIsMaskedQuote(int charIndex) => IsNotEndOfFile(charIndex + 1) && _fileContent[charIndex] == '"' && _fileContent[charIndex + 1] == '"';
    }
}