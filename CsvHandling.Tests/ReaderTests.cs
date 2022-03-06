using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DoenaSoft.CsvHandling.Tests
{
    [TestClass]
    public class ReaderTests
    {
        [TestMethod]
        public void StraightForward()
        {
            var sheet = (new CsvReader()).Read("StraightForward.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(2, sheet.RowCount);

            Assert.AreEqual("47", sheet[0, 0]);
            Assert.AreEqual("28", sheet[1, 0]);
            Assert.AreEqual("John", sheet[2, 0]);
            Assert.AreEqual("4.5", sheet[3, 0]);

            Assert.AreEqual("23", sheet[0, 1]);
            Assert.AreEqual("4.2", sheet[1, 1]);
            Assert.AreEqual("Jane", sheet[2, 1]);
            Assert.AreEqual("Hurz", sheet[3, 1]);
        }

        [TestMethod]
        public void MixedBag()
        {
            var sheet = (new CsvReader()).Read("MixedBag.csv", false, ',', Encoding.UTF8);

            AssertMixedBag(sheet);
        }

        [TestMethod]
        public void MixedBagWithEmptyEndLine()
        {
            var sheet = (new CsvReader()).Read("MixedBagWithEmptyEndLine.csv", false, ',', Encoding.UTF8);

            AssertMixedBag(sheet);
        }

        private static void AssertMixedBag(ICsvSheet sheet)
        {
            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(2, sheet.RowCount);

            Assert.AreEqual("47", sheet[0, 0]);
            Assert.AreEqual("28\"", sheet[1, 0]);
            Assert.AreEqual("John", sheet[2, 0]);
            Assert.AreEqual("4.5", sheet[3, 0]);

            Assert.AreEqual("23", sheet[0, 1]);
            Assert.AreEqual("4,2", sheet[1, 1]);
            Assert.AreEqual("Jane", sheet[2, 1]);
            Assert.AreEqual("Hurz", sheet[3, 1]);
        }

        [TestMethod]
        public void EdgeCaseMaskedQuote()
        {
            var sheet = (new CsvReader()).Read("EdgeCaseMaskedQuote.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(1, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual("\"", sheet[0, 0]);
        }

        [TestMethod]
        public void EdgeCaseSpacedQuote()
        {
            var sheet = (new CsvReader()).Read("EdgeCaseSpacedQuote.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(3, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual("23", sheet[0, 0]);
            Assert.AreEqual("Hurz", sheet[1, 0]);
            Assert.AreEqual("47", sheet[2, 0]);
        }

        [TestMethod]
        public void EmptyCell()
        {
            var sheet = (new CsvReader()).Read("EmptyCell.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(3, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual("23", sheet[0, 0]);
            Assert.AreEqual("", sheet[1, 0]);
            Assert.AreEqual("47", sheet[2, 0]);
        }

        [TestMethod]
        public void EmptyQuotedCell()
        {
            var sheet = (new CsvReader()).Read("EmptyQuotedCell.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual("23", sheet[0, 0]);
            Assert.AreEqual("", sheet[1, 0]);
            Assert.AreEqual("47", sheet[2, 0]);
            Assert.AreEqual("", sheet[3, 0]);
        }

        [TestMethod]
        public void EdgeCaseUnquotedSpace()
        {
            var sheet = (new CsvReader()).Read("EdgeCaseUnquotedSpace.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(3, sheet.ColumnCount);
            Assert.AreEqual(1, sheet.RowCount);

            Assert.AreEqual("23 ", sheet[0, 0]);
            Assert.AreEqual("Hurz", sheet[1, 0]);
            Assert.AreEqual(" 47", sheet[2, 0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCsvException))]
        public void EdgeCaseUnclosedQuote()
        {
            (new CsvReader()).Read("EdgeCaseUnclosedQuote.csv", false, ',', Encoding.UTF8);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCsvException))]
        public void EdgeCaseTextOutsideQuote()
        {
            (new CsvReader()).Read("EdgeCaseTextOutsideQuote.csv", false, ',', Encoding.UTF8);
        }

        [TestMethod]
        public void EdgeCaseQuotedMultiline()
        {
            var sheet = (new CsvReader()).Read("EdgeCaseQuotedMultiline.csv", false, ',', Encoding.UTF8);

            Assert.AreEqual(4, sheet.ColumnCount);
            Assert.AreEqual(2, sheet.RowCount);

            Assert.AreEqual("47", sheet[0, 0]);
            Assert.AreEqual("28\"", sheet[1, 0]);
            Assert.AreEqual("John", sheet[2, 0]);
            Assert.AreEqual("Räuber\r\nHotzenplotz", sheet[3, 0]);

            Assert.AreEqual("23", sheet[0, 1]);
            Assert.AreEqual("4,2", sheet[1, 1]);
            Assert.AreEqual("Jane", sheet[2, 1]);
            Assert.AreEqual("Hurz", sheet[3, 1]);
        }
    }
}