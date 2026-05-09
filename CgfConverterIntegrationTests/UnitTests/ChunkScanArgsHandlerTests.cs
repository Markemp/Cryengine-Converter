using System;
using System.IO;
using System.Linq;
using CgfConverter.Diagnostics.ChunkScanning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ChunkScanArgsHandlerTests
{
    [TestMethod]
    public void ProcessArgs_DirectInput_AddsInput()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual("ship.cgf", handler.Options.Inputs.Single());
    }

    [TestMethod]
    public void ProcessArgs_MultipleInputs_AddsInputs()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "ship.skin"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(2, handler.Options.Inputs.Count);
    }

    [TestMethod]
    public void ProcessArgs_Recursive_SetsRecursive()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["objects", "-recursive"]);

        Assert.AreEqual(0, result);
        Assert.IsTrue(handler.Options.Recursive);
    }

    [TestMethod]
    public void ProcessArgs_FormatJson_SetsJson()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-format", "json"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(ChunkScanReportFormat.Json, handler.Options.Format);
    }

    [TestMethod]
    public void ProcessArgs_FormatCsv_SetsCsv()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-format", "csv"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(ChunkScanReportFormat.Csv, handler.Options.Format);
    }

    [TestMethod]
    public void ProcessArgs_OutJson_InfersJson()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-out", "report.json"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(ChunkScanReportFormat.Json, handler.Options.Format);
    }

    [TestMethod]
    public void ProcessArgs_OutCsv_InfersCsv()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-out", "report.csv"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(ChunkScanReportFormat.Csv, handler.Options.Format);
    }

    [TestMethod]
    public void ProcessArgs_InvalidFormat_ReturnsError()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-format", "xml"], TextWriter.Null);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ProcessArgs_MaxThreadsZero_UsesProcessorCount()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-mt", "0"]);

        Assert.AreEqual(0, result);
        Assert.AreEqual(Environment.ProcessorCount, handler.Options.MaxThreads);
    }

    [TestMethod]
    public void ProcessArgs_NegativeMaxThreads_ReturnsError()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-mt", "-1"], TextWriter.Null);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ProcessArgs_NonNumericMaxThreads_ReturnsError()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-mt", "many"], TextWriter.Null);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ProcessArgs_UnknownDashOption_ReturnsError()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs(["ship.cgf", "-bad"], TextWriter.Null);

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void ProcessArgs_NoInput_ReturnsError()
    {
        var handler = new ChunkScanArgsHandler();

        var result = handler.ProcessArgs([], TextWriter.Null);

        Assert.AreEqual(1, result);
    }
}
