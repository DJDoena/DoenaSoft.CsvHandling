using System.Collections.Generic;

namespace DoenaSoft.CsvHandling;

/// <summary>
/// Returns a list of valid field delimiters
/// </summary>
public static class Delimiter
{
    private static readonly HashSet<char> _validDelimiters = new HashSet<char>
    {
        '\t',
        ' ',
        ',',
        ';',
        '|',
        '~',
        ':'
    };

    /// <summary>
    /// Returns a list of valid field delimiters
    /// </summary>
    public static IEnumerable<char> GetValidDelimiters() => _validDelimiters;

    /// <summary>
    /// Checks if a character is a valid delimiter
    /// </summary>
    public static bool IsValidDelimiter(char delimiter) => _validDelimiters.Contains(delimiter);
}