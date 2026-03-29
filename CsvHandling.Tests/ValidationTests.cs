using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DoenaSoft.CsvHandling.Tests;

[TestClass]
public class ValidationTests
{
    [TestMethod]
    public void InvalidCsvException_WithLineAndPosition_FormatsMessage()
    {
        var ex = new InvalidCsvException("Test error", 5, 123);

        Assert.AreEqual(5, ex.LineNumber);
        Assert.AreEqual(123, ex.CharPosition);
        Assert.IsTrue(ex.Message.Contains("Line: 5"));
        Assert.IsTrue(ex.Message.Contains("Position: 123"));
    }

    [TestMethod]
    public void InvalidCsvException_WithInnerException_PreservesInnerException()
    {
        var inner = new FormatException("Inner error");
        var ex = new InvalidCsvException("Test error", 5, 123, inner);

        Assert.AreEqual(5, ex.LineNumber);
        Assert.AreEqual(123, ex.CharPosition);
        Assert.AreSame(inner, ex.InnerException);
    }

    [TestMethod]
    public void CsvWriter_EmptySheet_ThrowsArgumentException()
    {
        var sheet = new CsvSheet(false);
        var writer = new CsvWriter();

        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            writer.Write("output.csv", sheet);
        });

        Assert.IsTrue(ex.Message.Contains("at least one column"));
    }

    [TestMethod]
    public void CsvWriter_SheetWithZeroRowsAndNoHeader_ThrowsArgumentException()
    {
        var sheet = new CsvSheet(false);
        sheet.AddColumn();

        var writer = new CsvWriter();

        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            writer.Write("output.csv", sheet);
        });

        Assert.IsTrue(ex.Message.Contains("at least one row"));
    }

    [TestMethod]
    public void CsvWriter_SheetWithOnlyHeader_Succeeds()
    {
        var sheet = new CsvSheet(true);
        sheet.AddHeaderRow(new[] { "Name", "Age" });

        var writer = new CsvWriter();
        var tempFile = Path.GetTempFileName();

        try
        {
            writer.Write(tempFile, sheet);

            var content = File.ReadAllText(tempFile);
            Assert.AreEqual("Name,Age", content.Trim());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void CsvReader_UnclosedQuote_ThrowsWithDiagnostics()
    {
        var ex = Assert.ThrowsExactly<InvalidCsvException>(() =>
        {
            (new CsvReader()).Read("EdgeCaseUnclosedQuote.csv", false, ',', Encoding.UTF8);
        });

        Assert.IsTrue(ex.LineNumber > 0, "LineNumber should be set");
        Assert.IsTrue(ex.CharPosition >= 0, "CharPosition should be set");
    }

    [TestMethod]
    public void CsvReader_TextOutsideQuote_ThrowsWithDiagnostics()
    {
        var ex = Assert.ThrowsExactly<InvalidCsvException>(() =>
        {
            (new CsvReader()).Read("EdgeCaseTextOutsideQuote.csv", false, ',', Encoding.UTF8);
        });

        Assert.IsTrue(ex.LineNumber > 0, "LineNumber should be set");
        Assert.IsTrue(ex.CharPosition >= 0, "CharPosition should be set");
    }

    [TestMethod]
    public void CsvReader_StrictMode_InconsistentRowLength_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<InvalidCsvException>(() =>
        {
            (new CsvReader()).Read("StrictModeTests.csv", true, ',', Encoding.UTF8, strictMode: true);
        });

        Assert.IsTrue(ex.Message.Contains("2 column(s)"), "Should indicate found 2 columns");
        Assert.IsTrue(ex.Message.Contains("3 column(s)"), "Should indicate expected 3 columns");
    }

    [TestMethod]
    public void CsvReader_LenientMode_InconsistentRowLength_PadsWithEmptyCells()
    {
        var sheet = (new CsvReader()).Read("StrictModeTests.csv", true, ',', Encoding.UTF8, strictMode: false);

        Assert.AreEqual(3, sheet.ColumnCount);
        Assert.AreEqual(2, sheet.RowCount);
        Assert.AreEqual("Jane", sheet["Name", 1]);
        Assert.AreEqual("25", sheet["Age", 1]);
        Assert.AreEqual("", sheet["City", 1]); // Padded with empty cell
    }

    [TestMethod]
    public void CsvReader_ClassicMacLineEndings_ParsesCorrectly()
    {
        var sheet = (new CsvReader()).Read("ClassicMacLineEndings.csv", true, ',', Encoding.UTF8);

        Assert.AreEqual(2, sheet.ColumnCount);
        Assert.AreEqual(2, sheet.RowCount);
        Assert.AreEqual("John", sheet["Name", 0]);
        Assert.AreEqual("30", sheet["Age", 0]);
        Assert.AreEqual("Jane", sheet["Name", 1]);
        Assert.AreEqual("25", sheet["Age", 1]);
    }

    [TestMethod]
    public void CsvSheet_DuplicateColumnNames_ThrowsException()
    {
        var sheet = new CsvSheet(true);

        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            sheet.AddHeaderRow(new[] { "Name", "Age", "Name" });
        });

        Assert.IsTrue(ex.Message.Contains("Duplicate column name"));
        Assert.IsTrue(ex.Message.Contains("Name"));
    }

    [TestMethod]
    public void CsvSheet_DuplicateColumnNames_CaseInsensitive_ThrowsException()
    {
        var sheet = new CsvSheet(true);

        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            sheet.AddHeaderRow(new[] { "Name", "Age", "name" });
        });

        Assert.IsTrue(ex.Message.Contains("Duplicate column name"));
    }

    [TestMethod]
    public void CsvHelper_StrictMode_WorksCorrectly()
    {
        var ex = Assert.ThrowsExactly<InvalidCsvException>(() =>
        {
            CsvHelper.Read("StrictModeTests.csv", true, ',', Encoding.UTF8, strictMode: true);
        });

        Assert.IsTrue(ex.Message.Contains("column(s)"));
    }

    [TestMethod]
    public void CsvReader_InvalidDelimiter_ThrowsWithHelpfulMessage()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            (new CsvReader()).Read("StraightForward.csv", false, '@', Encoding.UTF8);
        });

        Assert.IsTrue(ex.Message.Contains("valid delimiters"));
        Assert.IsTrue(ex.Message.Contains("comma"));
        Assert.IsTrue(ex.Message.Contains("tab"));
    }

    [TestMethod]
    public void CsvWriter_InvalidDelimiter_ThrowsWithHelpfulMessage()
    {
        var sheet = new CsvSheet(false);
        sheet.AddColumn();
        sheet.AddRow();

        var writer = new CsvWriter();

        var ex = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            writer.Write("output.csv", sheet, delimiter: '@');
        });

        Assert.IsTrue(ex.Message.Contains("valid delimiters"));
        Assert.IsTrue(ex.Message.Contains("comma"));
        Assert.IsTrue(ex.Message.Contains("semicolon"));
    }
}
