using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DoenaSoft.CsvHandling.Tests
{
    [TestClass]
    public class SheetTests
    {
        [TestMethod]
        public void AddHeaderRow()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string>() { "Header", "Header2", "Header3" });

            Assert.AreEqual(3, sheet.ColumnCount);
            Assert.AreEqual(0, sheet.RowCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(3, columnNames.Count);

            Assert.AreEqual("Header", columnNames[0]);
            Assert.AreEqual("Header2", columnNames[1]);
            Assert.AreEqual("Header3", columnNames[2]);
        }

        [TestMethod]
        public void AddLongerRowAndAccessPreviousRowNewColumnCellWithHeaderRow()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string> { "A", "B", "C" });

            sheet.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet.AddRow(new List<string>() { "A2", "B2", "C2" });
            sheet.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet.AddRow(new List<string>() { "A4", "B4", "C4", "D4", "E4" });

            Assert.AreEqual(5, sheet.ColumnCount);
            Assert.AreEqual(4, sheet.RowCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(5, columnNames.Count);

            Assert.AreEqual("A", columnNames[0]);
            Assert.AreEqual("B", columnNames[1]);
            Assert.AreEqual("C", columnNames[2]);
            Assert.AreEqual("Undefined", columnNames[3]);
            Assert.AreEqual("Undefined_1", columnNames[4]);

            var d2 = sheet[3, 1];

            Assert.IsTrue(string.IsNullOrEmpty(d2));

            var d4 = sheet[3, 3];

            Assert.AreEqual("D4", d4);

            var a5 = sheet["Undefined_1", 0];

            Assert.IsTrue(string.IsNullOrEmpty(a5));

            var e4 = sheet["Undefined_1", 3];

            Assert.AreEqual("E4", e4);
        }

        [TestMethod]
        public void AddShorterRowAndAccessPreviousRowNewColumnCellWithHeaderRow()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string> { "A", "B", "C" });

            sheet.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet.AddRow(new List<string>() { "A2", "B2", "C3" });
            sheet.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet.AddRow(new List<string>() { "A4", "B4" });

            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(4, sheet.RowCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(4, columnNames.Count);

            Assert.AreEqual("A", columnNames[0]);
            Assert.AreEqual("B", columnNames[1]);
            Assert.AreEqual("C", columnNames[2]);
            Assert.AreEqual("Undefined", columnNames[3]);

            var c4 = sheet[2, 3];

            Assert.IsTrue(string.IsNullOrEmpty(c4));

            var d4 = sheet["Undefined", 3];

            Assert.IsTrue(string.IsNullOrEmpty(d4));
        }

        [TestMethod]
        public void AddLongerRowAndAccessPreviousRowNewColumnCellWithoutHeaderRow()
        {
            var sheet = new CsvSheet(false);

            sheet.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet.AddRow(new List<string>() { "A2", "B2", "C2" });
            sheet.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet.AddRow(new List<string>() { "A4", "B4", "C4", "D4", "E4" });

            Assert.AreEqual(5, sheet.ColumnCount);
            Assert.AreEqual(4, sheet.RowCount);

            var c2 = sheet[3, 1];

            Assert.IsTrue(string.IsNullOrEmpty(c2));

            var d4 = sheet[3, 3];

            Assert.AreEqual("D4", d4);
        }

        [TestMethod]
        public void AddShorterRowAndAccessPreviousRowNewColumnCellWithoutHeaderRow()
        {
            var sheet = new CsvSheet(false);

            sheet.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet.AddRow(new List<string>() { "A2", "B2", "C3" });
            sheet.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet.AddRow(new List<string>() { "A4", "B4" });

            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(4, sheet.RowCount);

            var c4 = sheet[2, 3];

            Assert.IsTrue(string.IsNullOrEmpty(c4));
        }

        [TestMethod]
        public void UnquotedSpacedCell()
        {
            var sheet = new CsvSheet(false);

            sheet.AddRow(new List<string>() { " A1", " A2 ", "A3 " });

            Assert.AreEqual(3, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual(" A1", sheet[0, 0]);
            Assert.AreEqual(" A2 ", sheet[1, 0]);
            Assert.AreEqual("A3 ", sheet[2, 0]);
        }

        [TestMethod]
        public void RemoveSecondFromFourColumns()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string> { "A", "B", "C", "D" });

            sheet.RemoveColumn(1);

            Assert.AreEqual(3, sheet.ColumnCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(3, columnNames.Count);

            Assert.AreEqual("A", columnNames[0]);
            Assert.AreEqual("C", columnNames[1]);
            Assert.AreEqual("D", columnNames[2]);
        }

        [TestMethod]
        public void RemoveFourthFromFourColumns()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string> { "A", "B", "C", "D" });

            sheet.RemoveColumn(3);

            Assert.AreEqual(3, sheet.ColumnCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(3, columnNames.Count);

            Assert.AreEqual("A", columnNames[0]);
            Assert.AreEqual("B", columnNames[1]);
            Assert.AreEqual("C", columnNames[2]);
        }

        [TestMethod]
        public void RemoveNamedColumn()
        {
            var sheet = new CsvSheet(true);

            sheet.AddHeaderRow(new List<string> { "A", "B", "C", "D" });

            sheet.RemoveColumn("B");

            Assert.AreEqual(3, sheet.ColumnCount);

            var columnNames = sheet.GetColumnNames().ToList();

            Assert.AreEqual(3, columnNames.Count);

            Assert.AreEqual("A", columnNames[0]);
            Assert.AreEqual("C", columnNames[1]);
            Assert.AreEqual("D", columnNames[2]);
        }

        [TestMethod]
        public void RemoveSecondFromFourRows()
        {
            var sheet = new CsvSheet(false);

            sheet.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet.AddRow(new List<string>() { "A2", "B2", "C2" });
            sheet.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet.AddRow(new List<string>() { "A4", "B4", "C4", "D4", "E4" });

            sheet.RemoveRow(1);

            Assert.AreEqual(5, sheet.ColumnCount);
            Assert.AreEqual(3, sheet.RowCount);

            Assert.AreEqual("B3", sheet[1, 1]);
        }

        [TestMethod]
        public void GetSheet()
        {
            var sheet1 = new CsvSheet(true);

            sheet1.AddHeaderRow(new List<string> { "A", "B", "C" });

            sheet1.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet1.AddRow(new List<string>() { "A2", "B2", "C2" });
            sheet1.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet1.AddRow(new List<string>() { "A4", "B4", "C4", "D4", "E4" });

            var array1 = sheet1.GetSheet();

            Assert.AreEqual(5, array1.GetLength(0));
            Assert.AreEqual(5, array1.GetLength(1));

            var sheet2 = new CsvSheet(true);

            sheet2.AddHeaderRow(new List<string> { "A", "B", "C" });

            sheet2.AddRow(new List<string>() { "A1", "B1", "C1" });
            sheet2.AddRow(new List<string>() { "A2", "B2", "C3" });
            sheet2.AddRow(new List<string>() { "A3", "B3", "C3", "D3" });
            sheet2.AddRow(new List<string>() { "A4", "B4" });

            var array2 = sheet2.GetSheet();

            Assert.AreEqual(4, array2.GetLength(0));
            Assert.AreEqual(5, array2.GetLength(1));
        }
    }
}