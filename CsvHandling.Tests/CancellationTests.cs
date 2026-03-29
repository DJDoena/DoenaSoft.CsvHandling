using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DoenaSoft.CsvHandling.Tests;

[TestClass]
public class CancellationTests
{
    [TestMethod]
    public void CsvReader_CancellationToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() =>
        {
            (new CsvReader()).Read("StraightForward.csv", false, ',', Encoding.UTF8, cancellationToken: cts.Token);
        });
    }

    [TestMethod]
    public void CsvWriter_CancellationToken_ThrowsOperationCanceledException()
    {
        var sheet = new CsvSheet(false);
        sheet.AddColumn();
        sheet.AddRow(new[] { "Test" });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var tempFile = Path.GetTempFileName();

        try
        {
            Assert.ThrowsExactly<OperationCanceledException>(() =>
            {
                (new CsvWriter()).Write(tempFile, sheet, ',', Encoding.UTF8, cts.Token);
            });
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
    public void CsvHelper_Read_CancellationToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() =>
        {
            CsvHelper.Read("StraightForward.csv", false, cancellationToken: cts.Token);
        });
    }

    [TestMethod]
    public void CsvHelper_Write_CancellationToken_ThrowsOperationCanceledException()
    {
        var sheet = new CsvSheet(false);
        sheet.AddColumn();
        sheet.AddRow(new[] { "Test" });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var tempFile = Path.GetTempFileName();

        try
        {
            Assert.ThrowsExactly<OperationCanceledException>(() =>
            {
                CsvHelper.Write(tempFile, sheet, cancellationToken: cts.Token);
            });
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
    public void CsvReader_NoCancellation_CompletesSuccessfully()
    {
        using var cts = new CancellationTokenSource();

        var sheet = (new CsvReader()).Read("StraightForward.csv", false, ',', Encoding.UTF8, cancellationToken: cts.Token);

        Assert.IsNotNull(sheet);
        Assert.AreEqual(4, sheet.ColumnCount);
        Assert.AreEqual(2, sheet.RowCount);
    }

    [TestMethod]
    public void CsvWriter_NoCancellation_CompletesSuccessfully()
    {
        var sheet = new CsvSheet(false);
        sheet.AddColumn();
        sheet.AddRow(new[] { "Test" });

        using var cts = new CancellationTokenSource();

        var tempFile = Path.GetTempFileName();

        try
        {
            (new CsvWriter()).Write(tempFile, sheet, ',', Encoding.UTF8, cts.Token);

            Assert.IsTrue(File.Exists(tempFile));
            var content = File.ReadAllText(tempFile);
            Assert.AreEqual("Test", content.Trim());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
