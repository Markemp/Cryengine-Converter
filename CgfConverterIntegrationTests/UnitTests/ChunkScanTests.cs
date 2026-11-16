using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ChunkScanTests
{
    [TestMethod]
    public void ChunkFactory_ClassifiesTypesAndPreservesLegacyRandomApi()
    {
        var parsed = Chunk.New(ChunkType.Mesh, 0x800);
        Assert.AreEqual(
            ChunkFactoryClassification.ParserAvailable,
            ChunkFactoryClassifier.Classify(ChunkType.Mesh, parsed));

        var ignoredTypes = new[]
        {
            ChunkType.IvoAssetMetadata,
            ChunkType.IvoLodDistances,
            ChunkType.IvoLodMeshData,
            ChunkType.IvoBoundingData,
            ChunkType.IvoChunkTerminator,
            ChunkType.IvoMtlNameVariant,
        };
        foreach (var ignoredType in ignoredTypes)
        {
            var ignored = Chunk.New(ignoredType, 1);
            Assert.AreEqual(
                ChunkFactoryClassification.IntentionallyIgnored,
                ChunkFactoryClassifier.Classify(ignoredType, ignored));
            Assert.IsInstanceOfType(ignored, typeof(ChunkUnknown));
        }

        var enumDefined = Chunk.New(ChunkType.FaceMap, 1);
        Assert.AreEqual(
            ChunkFactoryClassification.EnumDefinedUnimplemented,
            ChunkFactoryClassifier.Classify(ChunkType.FaceMap, enumDefined));

        var rawUnknownType = (ChunkType)0xDEADBEEFu;
        var rawUnknown = Chunk.New(rawUnknownType, 1);
        Assert.AreEqual(
            ChunkFactoryClassification.RawUnknown,
            ChunkFactoryClassifier.Classify(rawUnknownType, rawUnknown));
        Assert.ThrowsException<NotSupportedException>(() =>
            Chunk.New(ChunkType.Mesh, 0x7FFF));

        Assert.IsNotNull(typeof(Chunk).GetMethod(
            nameof(Chunk.GetNextRandom),
            BindingFlags.Public | BindingFlags.Static));
        var randomField = typeof(Chunk).GetField("rnd", BindingFlags.NonPublic | BindingFlags.Static);
        var pickedField = typeof(Chunk).GetField("alreadyPickedRandoms", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(randomField);
        Assert.IsNotNull(pickedField);
        Assert.IsTrue(randomField.IsFamily);
        Assert.IsTrue(pickedField.IsFamily);
    }

    [TestMethod]
    public void DiagnosticFailure_RemovesFailedChunkAndRecoversNextChunk()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(BuildTimingFailureThenRawUnknown746());

        var model = Model.FromStream("fixture.cgf", stream, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.IsTrue(result.Complete);
        Assert.AreEqual(2, result.ObservedChunkCount);
        Assert.AreEqual(1, result.ChunkOutcomes.ParseFailure);
        Assert.AreEqual(1, result.ChunkOutcomes.RawUnknown);
        Assert.AreEqual(result.ObservedChunkCount, result.ChunkOutcomes.Total);
        Assert.AreEqual(ModelReadStage.ChunkRead, result.Findings[0].Stage);
        Assert.AreEqual(44, result.Findings[0].ChunkId);
        Assert.IsFalse(model.ChunkMap.ContainsKey(44));
        Assert.IsTrue(model.ChunkMap.ContainsKey(45));
    }

    [TestMethod]
    public void NormalModelRead_RemainsFailFast()
    {
        using var stream = new MemoryStream(BuildTimingFailureThenRawUnknown746());
        Exception failure = null;
        try
        {
            Model.FromStream("fixture.cgf", stream);
        }
        catch (Exception exception)
        {
            failure = exception;
        }

        Assert.IsNotNull(failure);
    }

    [TestMethod]
    public void DiagnosticAndNormal_ProduceEquivalentValidParsedChunkAndSeeks()
    {
        var bytes = BuildSourceInfo746();
        var normalTracking = new TrackingStream(new MemoryStream(bytes));
        var diagnosticTracking = new TrackingStream(new MemoryStream(bytes));

        var normal = Model.FromStream("fixture.cgf", normalTracking);
        var observer = new ChunkScanObserver();
        var diagnostic = Model.FromStream("fixture.cgf", diagnosticTracking, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", diagnostic);

        var normalChunk = (ChunkSourceInfo)normal.ChunkMap[42];
        var diagnosticChunk = (ChunkSourceInfo)diagnostic.ChunkMap[42];
        Assert.AreEqual(normalChunk.GetType(), diagnosticChunk.GetType());
        Assert.AreEqual("source", normalChunk.SourceFile);
        Assert.AreEqual(normalChunk.SourceFile, diagnosticChunk.SourceFile);
        Assert.AreEqual(normal.FileSignature, diagnostic.FileSignature);
        Assert.AreEqual(normal.NumChunks, diagnostic.NumChunks);
        Assert.AreEqual(1, result.ChunkOutcomes.Parsed);
        Assert.AreEqual(0, result.Findings.Count);
        CollectionAssert.AreEqual(normalTracking.Seeks.ToArray(), diagnosticTracking.Seeks.ToArray());
    }

    [TestMethod]
    public void DiagnosticModelRead_ContinuesAfterUnsupportedVersion()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(BuildUnsupportedVersionThenRawUnknown746());

        var model = Model.FromStream("fixture.cgf", stream, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.AreEqual(2, result.ObservedChunkCount);
        Assert.AreEqual(ChunkScanOutcome.UnsupportedVersion, result.Findings[0].Outcome);
        Assert.AreEqual(ModelReadStage.ChunkFactory, result.Findings[0].Stage);
        Assert.AreEqual(ChunkScanOutcome.RawUnknown, result.Findings[1].Outcome);
    }

    [TestMethod]
    public void InvalidSignature_IsAHeaderFindingWithRawSignature()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("BadSign!"));

        Assert.ThrowsException<NotSupportedException>(() =>
            Model.FromStream("fixture.cgf", stream, observer));

        var result = observer.CreateFileResult(1, "fixture.cgf", model: null);
        Assert.IsFalse(result.Complete);
        Assert.AreEqual("BadSign!", result.FileSignature);
        Assert.AreEqual(1, result.Findings.Count);
        Assert.AreEqual(ModelReadStage.FileHeader, result.Findings[0].Stage);
        Assert.AreEqual(ChunkScanOutcome.UnsupportedFileSignature, result.Findings[0].Outcome);
        Assert.AreEqual(ChunkScanFindingScope.File, result.Findings[0].Scope);
        Assert.AreEqual(1, result.FileOutcomes.UnsupportedFileSignature);
        Assert.AreEqual(0, result.ChunkOutcomes.Total);
    }

    [TestMethod]
    public void TruncatedTable_PreservesHeaderSnapshot()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(BuildTruncatedTable746());

        Assert.ThrowsException<EndOfStreamException>(() =>
            Model.FromStream("fixture.cgf", stream, observer));

        var result = observer.CreateFileResult(1, "fixture.cgf", model: null);
        Assert.IsFalse(result.Complete);
        Assert.AreEqual("CrCh", result.FileSignature);
        Assert.AreEqual("0x00000746", result.FileVersion);
        Assert.AreEqual(1u, result.DeclaredChunkCount);
        Assert.AreEqual(16, result.ChunkTableOffset);
        Assert.AreEqual(ModelReadStage.ChunkTable, result.Findings.Single().Stage);
        Assert.AreEqual(1, result.FileOutcomes.ParseFailure);
        Assert.AreEqual(0, result.ChunkOutcomes.Total);
    }

    [TestMethod]
    public void TruncatedHeader_PreservesFieldsReadBeforeFailure()
    {
        using var bytes = new MemoryStream();
        using (var writer = new BinaryWriter(bytes, Encoding.ASCII, leaveOpen: true))
        {
            writer.Write(Encoding.ASCII.GetBytes("CrCh"));
            writer.Write((uint)FileVersion.x0746);
        }
        bytes.Position = 0;
        var observer = new ChunkScanObserver();

        Assert.ThrowsException<EndOfStreamException>(() =>
            Model.FromStream("fixture.cgf", bytes, observer));

        var result = observer.CreateFileResult(1, "fixture.cgf", model: null);
        Assert.AreEqual("CrCh", result.FileSignature);
        Assert.AreEqual("0x00000746", result.FileVersion);
        Assert.IsNull(result.DeclaredChunkCount);
        Assert.IsNull(result.ChunkTableOffset);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Diagnostic900Ids_AreStableNegativeWhileNormalModePreservesLegacyRange()
    {
        var bytes = Build900(
            [
                (0x11223344u, 1u, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }),
                (0x11223344u, 1u, new byte[] { 0xCA, 0xFE, 0xBA, 0xBE }),
            ]);

        var randomField = typeof(Chunk).GetField("rnd", BindingFlags.NonPublic | BindingFlags.Static)!;
        var pickedField = typeof(Chunk).GetField("alreadyPickedRandoms", BindingFlags.NonPublic | BindingFlags.Static)!;
        var originalRandom = randomField.GetValue(null);
        var originalPicked = pickedField.GetValue(null);
        var expected = GetExpectedRandomIds(seed: 260, count: 2);
        Model normal;
        try
        {
            randomField.SetValue(null, new Random(260));
            pickedField.SetValue(null, new HashSet<int>());
            using var normalStream = new MemoryStream(bytes);
            normal = Model.FromStream("fixture.cgf", normalStream);
        }
        finally
        {
            randomField.SetValue(null, originalRandom);
            pickedField.SetValue(null, originalPicked);
        }
        CollectionAssert.AreEqual(expected, normal.ChunkMap.Keys.ToArray());

        var firstObserver = new ChunkScanObserver();
        using var firstStream = new MemoryStream(bytes);
        var firstModel = Model.FromStream("fixture.cgf", firstStream, firstObserver);
        var firstReport = CreateSingleFileReport(firstObserver.CreateFileResult(1, "fixture.cgf", firstModel));

        var secondObserver = new ChunkScanObserver();
        using var secondStream = new MemoryStream(bytes);
        var secondModel = Model.FromStream("fixture.cgf", secondStream, secondObserver);
        var secondReport = CreateSingleFileReport(secondObserver.CreateFileResult(1, "fixture.cgf", secondModel));

        CollectionAssert.AreEqual(
            new[] { int.MinValue, int.MinValue + 1 },
            firstModel.ChunkMap.Keys.OrderBy(value => value).ToArray());
        CollectionAssert.AreEqual(
            ChunkScanJsonWriter.Serialize(firstReport),
            ChunkScanJsonWriter.Serialize(secondReport));

        using var document = JsonDocument.Parse(ChunkScanJsonWriter.Serialize(firstReport));
        var findings = document.RootElement.GetProperty("files")[0].GetProperty("findings");
        Assert.AreEqual(JsonValueKind.Null, findings[0].GetProperty("chunkId").ValueKind);
        Assert.AreEqual(JsonValueKind.Null, findings[1].GetProperty("size").ValueKind);
    }

    [TestMethod]
    public void DiagnosticModelRead_CountsIntentionallyIgnoredChunksWithoutFindings()
    {
        var ignoredTypes = new[]
        {
            (uint)ChunkType.IvoAssetMetadata,
            (uint)ChunkType.IvoLodDistances,
            (uint)ChunkType.IvoLodMeshData,
            (uint)ChunkType.IvoBoundingData,
            (uint)ChunkType.IvoChunkTerminator,
            (uint)ChunkType.IvoMtlNameVariant,
        };
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(Build900(
            ignoredTypes.Select(type => (type, 1u, new byte[] { 0x00 })).ToArray()));

        var model = Model.FromStream("fixture.cgf", stream, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.AreEqual(6, result.ObservedChunkCount);
        Assert.AreEqual(6, result.ChunkOutcomes.IntentionallyIgnored);
        Assert.AreEqual(0, result.Findings.Count);
    }

    [TestMethod]
    public void DiagnosticIvoUnknownDataStream_IsANestedFindingWithoutChangingNormalRead()
    {
        const uint unknownDataStreamType = 0x770A9748u;
        var bytes = Build900(
            [
                (
                    (uint)ChunkType.IvoSkin,
                    0x900u,
                    BuildIvoSkinUnknownDataStreamBody(unknownDataStreamType)),
            ]);

        using var normalStream = new MemoryStream(bytes);
        var normal = Model.FromStream("fixture.skinm", normalStream);
        Assert.IsInstanceOfType(normal.ChunkMap.Values.Single(), typeof(ChunkIvoSkinMesh));

        var observer = new ChunkScanObserver();
        using var diagnosticStream = new MemoryStream(bytes);
        var diagnostic = Model.FromStream("fixture.skinm", diagnosticStream, observer);
        var result = observer.CreateFileResult(1, "fixture.skinm", diagnostic);
        var report = CreateSingleFileReport(result);

        Assert.AreEqual(1, result.ObservedChunkCount);
        Assert.AreEqual(1, result.ChunkOutcomes.Parsed);
        Assert.AreEqual(0, result.ChunkOutcomes.RawUnknown);
        Assert.AreEqual(result.ObservedChunkCount, result.ChunkOutcomes.Total);
        var finding = result.Findings.Single();
        Assert.AreEqual(ChunkScanFindingScope.DataStream, finding.Scope);
        Assert.AreEqual(ChunkScanOutcome.RawUnknown, finding.Outcome);
        Assert.AreEqual(ModelReadStage.DataStreamRead, finding.Stage);
        Assert.AreEqual(0, finding.TableIndex);
        Assert.AreEqual("0x770A9748", finding.TypeCode);
        Assert.IsNull(finding.TypeName);
        Assert.AreEqual("0x00000900", finding.VersionRaw);
        Assert.AreEqual("0x000000C8", finding.Offset);
        Assert.AreEqual(1, report.Summary.NestedDataStreamFindings);
        Assert.AreEqual(1, report.Summary.FindingCount);
        Assert.AreEqual(1, report.FindingGroups.Single().OccurrenceCount);

        using var document = JsonDocument.Parse(ChunkScanJsonWriter.Serialize(report));
        var root = document.RootElement;
        Assert.AreEqual(4, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual(
            1,
            root.GetProperty("summary").GetProperty("nestedDataStreamFindings").GetInt32());
        Assert.AreEqual(
            "dataStream",
            root.GetProperty("files")[0].GetProperty("findings")[0].GetProperty("scope").GetString());
    }

    [TestMethod]
    public void DiagnosticRecovery_RestoresEndiannessAfterFailure()
    {
        var bytes = Build900(
            [
                ((uint)ChunkType.Timing, 0x80000918u, new byte[4]),
                ((uint)ChunkType.Mesh, 0x900u, BuildMesh900Body()),
            ]);
        var wrapped = new ThrowAtPositionStream(
            new MemoryStream(bytes),
            48,
            () => new EndOfStreamException("synthetic parser failure"));
        var observer = new ChunkScanObserver();

        var model = Model.FromStream("fixture.cgf", wrapped, observer);
        var mesh = (ChunkMesh)model.ChunkMap[int.MinValue + 1];
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.AreEqual(0x01020304, mesh.Flags2);
        Assert.AreEqual(2, mesh.NumVertices);
        Assert.AreEqual(3, mesh.NumIndices);
        Assert.AreEqual(4u, mesh.NumVertSubsets);
        Assert.AreEqual(1, result.ChunkOutcomes.ParseFailure);
        Assert.IsTrue(wrapped.Seeks.Any(seek => seek.Offset == 52 && seek.Origin == SeekOrigin.Begin));
    }

    [TestMethod]
    public void DiagnosticSkipFailure_IsAttributedAndRecoveryContinues()
    {
        var bytes = Build746(
            [
                (0x1234, 0x2345, 42, new byte[] { 1, 2, 3, 4 }),
                (0x1234, 0x2345, 43, new byte[] { 5, 6, 7, 8 }),
            ]);
        var wrapped = new ThrowOnceOnCurrentSeekStream(new MemoryStream(bytes));
        var observer = new ChunkScanObserver();

        var model = Model.FromStream("fixture.cgf", wrapped, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.IsTrue(wrapped.Threw);
        Assert.AreEqual(ModelReadStage.ChunkSkip, result.Findings[0].Stage);
        Assert.IsFalse(model.ChunkMap.ContainsKey(42));
        Assert.IsTrue(model.ChunkMap.ContainsKey(43));
    }

    [TestMethod]
    public void Caf744Correction_PreservesDeclaredCountAndUnknownFinalSize()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(Build744Caf());

        var model = Model.FromStream("fixture.CAF", stream, observer);
        var result = observer.CreateFileResult(1, "fixture.CAF", model);

        Assert.AreEqual(2u, model.NumChunks);
        Assert.AreEqual(999u, result.DeclaredChunkCount);
        Assert.AreEqual(2, result.ObservedChunkCount);
        Assert.AreEqual(16u, result.Findings[0].Size);
        Assert.IsNull(result.Findings[1].Size);
    }

    [TestMethod]
    public void Format745_ParsesEmbeddedHeader()
    {
        var observer = new ChunkScanObserver();
        using var stream = new MemoryStream(Build745());

        var model = Model.FromStream("fixture.cgf", stream, observer);
        var result = observer.CreateFileResult(1, "fixture.cgf", model);

        Assert.AreEqual(1, result.ObservedChunkCount);
        Assert.AreEqual(ChunkScanOutcome.RawUnknown, result.Findings[0].Outcome);
        Assert.AreEqual(16u, result.Findings[0].Size);
    }

    [TestMethod]
    public void CommandDispatch_RequiresExactFirstTokenAndSlicesArguments()
    {
        string[] received = null;
        var dispatched = ChunkScanCommand.TryDispatch(
            ["SCAN-UNKNOWN-CHUNKS", "one.cgf", "-out", "report.json"],
            arguments =>
            {
                received = arguments;
                return 17;
            },
            out var exitCode);

        Assert.IsTrue(dispatched);
        Assert.AreEqual(17, exitCode);
        CollectionAssert.AreEqual(new[] { "one.cgf", "-out", "report.json" }, received);

        var runnerCalled = false;
        dispatched = ChunkScanCommand.TryDispatch(
            ["one.cgf", "scan-unknown-chunks"],
            _ =>
            {
                runnerCalled = true;
                return 0;
            },
            out exitCode);
        Assert.IsFalse(dispatched);
        Assert.IsFalse(runnerCalled);
    }

    [TestMethod]
    public void CommandSyntax_RejectsInvalidOptionsAndAcceptsSoleHelp()
    {
        foreach (var help in new[] { "-usage", "--help", "-h" })
        {
            var exitCode = ChunkScanCommand.Run(
                [help],
                new StringWriter(),
                new StringWriter(),
                new ThrowingFileSystem(),
                _ => throw new AssertFailedException("Destination should not be reserved for help."));
            Assert.AreEqual(0, exitCode);
        }

        var invalidCases = new[]
        {
            new[] { "fixture.cgf" },
            new[] { "fixture.cgf", "-out", "fixture.cgf" },
            new[] { "fixture.cgf", "-out", "-recursive" },
            new[] { "fixture.cgf", "-recursive", "-recursive", "-out", "report.json" },
            new[] { "fixture.cgf", "-out", "one.json", "-out", "two.json" },
            new[] { "fixture.cgf", "--unknown", "-out", "report.json" },
            new[] { "fixture.cgf", "-usage", "-out", "report.json" },
        };

        foreach (var args in invalidCases)
        {
            var exitCode = ChunkScanCommand.Run(
                args,
                new StringWriter(),
                new StringWriter(),
                new ThrowingFileSystem(),
                _ => throw new AssertFailedException("Invalid syntax must not reserve output."));
            Assert.AreEqual(2, exitCode, string.Join(' ', args));
        }
    }

    [TestMethod]
    public void CommandRun_WritesDeterministicRelativeLfOnlyBomlessJsonAndCleansTemp()
    {
        var directory = CreateTempDirectory();
        try
        {
            var inputPath = Path.Combine(directory, "fixture.cgf");
            var reportPath = Path.Combine(directory, "report.json");
            File.WriteAllBytes(inputPath, BuildTimingFailureThenRawUnknown746());

            var firstExitCode = RunCommand(inputPath, reportPath, out var firstOutput, out var firstError);
            var firstBytes = File.ReadAllBytes(reportPath);
            var secondExitCode = RunCommand(inputPath, reportPath, out _, out _);
            var secondBytes = File.ReadAllBytes(reportPath);

            Assert.AreEqual(0, firstExitCode);
            Assert.AreEqual(0, secondExitCode);
            Assert.AreEqual(string.Empty, firstError);
            StringAssert.Contains(firstOutput, "found 2 diagnostic issue(s)");
            CollectionAssert.AreEqual(firstBytes, secondBytes);
            Assert.IsFalse(firstBytes.Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }));
            Assert.IsFalse(firstBytes.Contains((byte)'\r'));
            Assert.AreEqual(0, Directory.EnumerateFiles(directory, ".*.tmp").Count());

            var json = Encoding.UTF8.GetString(firstBytes);
            Assert.IsFalse(json.Contains(directory, StringComparison.OrdinalIgnoreCase));
            using var document = JsonDocument.Parse(firstBytes);
            AssertJsonContainsNoStringFragment(document.RootElement, Path.GetFileName(directory));
            var root = document.RootElement;
            Assert.AreEqual(4, root.GetProperty("schemaVersion").GetInt32());
            Assert.IsFalse(string.IsNullOrWhiteSpace(root.GetProperty("converterVersion").GetString()));
            Assert.AreEqual("input-1", root.GetProperty("inputs")[0].GetProperty("alias").GetString());
            Assert.AreEqual("fixture.cgf", root.GetProperty("files")[0].GetProperty("path").GetString());
            Assert.AreEqual("chunk", root.GetProperty("files")[0].GetProperty("findings")[0].GetProperty("scope").GetString());
            Assert.IsFalse(root.GetProperty("files")[0].GetProperty("findings")[0].TryGetProperty("message", out _));
            Assert.AreEqual(1, root.GetProperty("summary").GetProperty("chunkOutcomes").GetProperty("parseFailure").GetInt32());
            Assert.AreEqual(1, root.GetProperty("summary").GetProperty("chunkOutcomes").GetProperty("rawUnknown").GetInt32());
            Assert.IsFalse(root.GetProperty("summary").TryGetProperty("outcomes", out _));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Json_KeepsConverterBuildMetadataReadable()
    {
        var report = new ChunkScanReport
        {
            ConverterVersion = "2.0.0+test",
            Inputs = [],
            Recursive = false,
            ConsecutiveResourceExhaustionAbortThreshold = ChunkScanService.MaxConsecutiveResourceExhaustions,
        };
        report.FinalizeReport();

        var json = Encoding.UTF8.GetString(ChunkScanJsonWriter.Serialize(report));
        StringAssert.Contains(json, "2.0.0+test");
        Assert.IsFalse(json.Contains("\\u002B", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void HeaderTableFailure_IsFindingExitZeroAndWritesPartialReport()
    {
        var directory = CreateTempDirectory();
        try
        {
            var inputPath = Path.Combine(directory, "fixture.cgf");
            File.WriteAllBytes(inputPath, BuildTruncatedTable746());
            var destination = new CaptureDestination(Path.Combine(directory, "report.json"));
            var output = new StringWriter();
            var error = new StringWriter();

            var exitCode = ChunkScanCommand.Run(
                [inputPath, "-out", destination.OutputPath],
                output,
                error,
                new SystemChunkScanFileSystem(),
                _ => destination);

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(string.Empty, error.ToString());
            Assert.IsNotNull(destination.Report);
            Assert.AreEqual(0, destination.Report.OperationalErrors.Count);
            Assert.AreEqual(0, destination.Report.Summary.ChunkOutcomes.Total);
            Assert.AreEqual(1, destination.Report.Summary.FileOutcomes.ParseFailure);
            var file = destination.Report.Files.Single();
            Assert.AreEqual("CrCh", file.FileSignature);
            Assert.AreEqual(1u, file.DeclaredChunkCount);
            Assert.AreEqual(ModelReadStage.ChunkTable, file.Findings.Single().Stage);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void CommandRun_MissingInputProducesOperationalExitOneWithoutAbsolutePathLeak()
    {
        var directory = CreateTempDirectory();
        try
        {
            var missingPath = Path.Combine(directory, "missing.cgf");
            var destination = new CaptureDestination(Path.Combine(directory, "report.json"));
            var error = new StringWriter();

            var exitCode = ChunkScanCommand.Run(
                [missingPath, "-out", destination.OutputPath],
                new StringWriter(),
                error,
                new SystemChunkScanFileSystem(),
                _ => destination);

            Assert.AreEqual(1, exitCode);
            Assert.AreEqual(1, destination.Report.OperationalErrors.Count);
            Assert.AreEqual(0, destination.Report.Files.Count);
            var json = Encoding.UTF8.GetString(ChunkScanJsonWriter.Serialize(destination.Report));
            Assert.IsFalse(json.Contains(directory, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Service_RecursionIsExplicit()
    {
        var directory = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(directory, "nested");
            Directory.CreateDirectory(nested);
            File.WriteAllBytes(Path.Combine(nested, "fixture.cgf"), BuildRawUnknown746());
            var service = new ChunkScanService(new SystemChunkScanFileSystem());

            var topLevel = service.Scan([directory], recursive: false);
            var recursive = service.Scan([directory], recursive: true);

            Assert.AreEqual(0, topLevel.Files.Count);
            Assert.AreEqual(1, topLevel.OperationalErrors.Count);
            Assert.AreEqual(1, recursive.Files.Count);
            Assert.AreEqual(0, recursive.OperationalErrors.Count);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [DataTestMethod]
    [DataRow(".cgfm")]
    [DataRow(".cgam")]
    [DataRow(".chrm")]
    [DataRow(".skinm")]
    public void Service_AcceptsCompanionFilesWithoutBroadeningNormalConversion(string extension)
    {
        var fileSystem = new MemoryFileSystem();
        var path = $"fixture{extension}";
        fileSystem.AddFile(path, () => new MemoryStream(BuildRawUnknown746(), writable: false));

        var report = new ChunkScanService(fileSystem).Scan([path], recursive: false);

        Assert.IsFalse(CryEngine.SupportsFile(path));
        Assert.IsTrue(ChunkScanService.SupportsFile(path));
        Assert.AreEqual(1, report.Files.Count);
        Assert.AreEqual(extension, report.Files[0].Extension);
        Assert.AreEqual(0, report.OperationalErrors.Count);
    }

    [TestMethod]
    public void Service_DiscoversCompanionFilesAtRequestedDepth()
    {
        var directory = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(directory, "nested");
            Directory.CreateDirectory(nested);
            var topLevelPath = Path.Combine(directory, "top.cgfm");
            var nestedPath = Path.Combine(nested, "nested.cgam");
            File.WriteAllBytes(topLevelPath, BuildRawUnknown746());
            File.WriteAllBytes(nestedPath, BuildRawUnknown746());
            File.WriteAllBytes(Path.Combine(directory, "ignored.p4k"), BuildRawUnknown746());
            var service = new ChunkScanService(new SystemChunkScanFileSystem());

            var topLevel = service.Scan([directory], recursive: false);
            var recursive = service.Scan([directory], recursive: true);

            CollectionAssert.AreEqual(new[] { "top.cgfm" }, topLevel.Files.Select(file => file.Path).ToArray());
            CollectionAssert.AreEqual(new[] { "nested/nested.cgam", "top.cgfm" }, recursive.Files.Select(file => file.Path).ToArray());
            Assert.AreEqual(0, topLevel.OperationalErrors.Count);
            Assert.AreEqual(0, recursive.OperationalErrors.Count);
            Assert.IsFalse(CryEngine.SupportsFile(topLevelPath));
            Assert.IsFalse(CryEngine.SupportsFile(nestedPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void Service_ClassifiesUnauthorizedOpenAsFileOpenFailure()
    {
        var fileSystem = new MemoryFileSystem();
        fileSystem.AddFile("fixture.cgf", () => throw new UnauthorizedAccessException("private path"));

        var report = new ChunkScanService(fileSystem).Scan(["fixture.cgf"], recursive: false);

        Assert.AreEqual(1, report.Files.Count);
        Assert.IsFalse(report.Files[0].Complete);
        Assert.AreEqual(ChunkScanOperationalStage.FileOpen, report.OperationalErrors.Single().Stage);
        Assert.AreEqual("The file could not be opened.", report.OperationalErrors.Single().Message);
        Assert.AreEqual(1, report.Files[0].FileOutcomes.IoFailure);
        Assert.AreEqual(ChunkScanFindingScope.File, report.Files[0].Findings.Single().Scope);
        Assert.AreEqual(ModelReadStage.FileOpen, report.Files[0].Findings.Single().Stage);
    }

    [TestMethod]
    public void OutputPreflightFailure_HappensBeforeDiscoveryAndPreservesExistingReport()
    {
        var directory = CreateTempDirectory();
        try
        {
            var reportPath = Path.Combine(directory, "report.json");
            var original = Encoding.UTF8.GetBytes("original report");
            File.WriteAllBytes(reportPath, original);
            var fileSystem = new NoDiscoveryFileSystem();
            using var reportLock = new FileStream(reportPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var exitCode = ChunkScanCommand.Run(
                ["fixture.cgf", "-out", reportPath],
                new StringWriter(),
                new StringWriter(),
                fileSystem,
                ChunkScanJsonWriter.Reserve);

            Assert.AreEqual(1, exitCode);
            Assert.IsFalse(fileSystem.DiscoveryTouched);
            CollectionAssert.AreEqual(original, File.ReadAllBytes(reportPath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void CommitFailure_PreservesExistingReportPrintsSummaryAndCleansTemp()
    {
        var directory = CreateTempDirectory();
        try
        {
            var inputPath = Path.Combine(directory, "fixture.cgf");
            var reportPath = Path.Combine(directory, "report.json");
            var original = Encoding.UTF8.GetBytes("original report");
            File.WriteAllBytes(inputPath, BuildRawUnknown746());
            File.WriteAllBytes(reportPath, original);
            var output = new StringWriter();
            var error = new StringWriter();

            var exitCode = ChunkScanCommand.Run(
                [inputPath, "-out", reportPath],
                output,
                error,
                new SystemChunkScanFileSystem(),
                path => new LockedCommitDestination(ChunkScanJsonWriter.Reserve(path), path));

            Assert.AreEqual(1, exitCode);
            StringAssert.Contains(output.ToString(), "Discovered 1 file(s)");
            StringAssert.Contains(error.ToString(), "Failed to write scan report");
            CollectionAssert.AreEqual(original, File.ReadAllBytes(reportPath));
            Assert.AreEqual(0, Directory.EnumerateFiles(directory, ".*.tmp").Count());
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void ResourceExhaustion_SkipsOneFileAndContinuesCorpus()
    {
        var fileSystem = new MemoryFileSystem();
        var firstPath = fileSystem.FullPath("first.cgf");
        fileSystem.AddFile(
            "first.cgf",
            () => new ThrowAtPositionStream(
                new MemoryStream(BuildTiming746()),
                32,
                () => new OutOfMemoryException($"synthetic at {firstPath}")));
        fileSystem.AddFile("second.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new CaptureDestination(@"D:\virtual\report.json");

        var exitCode = ChunkScanCommand.Run(
            ["first.cgf", "second.cgf", "-out", "report.json"],
            new StringWriter(),
            new StringWriter(),
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.IsFalse(destination.Report.Aborted);
        Assert.AreEqual(2, destination.Report.Summary.FilesDiscovered);
        Assert.AreEqual(2, destination.Report.Summary.FilesProcessed);
        Assert.AreEqual(1, destination.Report.OperationalErrors.Count);
        Assert.AreEqual(ChunkScanOperationalStage.ResourceExhaustion, destination.Report.OperationalErrors[0].Stage);
        var resourceFinding = destination.Report.Files[0].Findings.Single();
        Assert.AreEqual(ChunkScanOutcome.ResourceExhaustion, resourceFinding.Outcome);
        Assert.AreEqual(0, resourceFinding.TableIndex);
        Assert.AreEqual(ModelReadStage.ChunkRead, resourceFinding.Stage);
        Assert.AreEqual(1, destination.Report.Files[0].ChunkOutcomes.ResourceExhaustion);
        CollectionAssert.AreEqual(
            new[] { fileSystem.FullPath("first.cgf"), fileSystem.FullPath("second.cgf") },
            fileSystem.OpenedPaths.ToArray());
    }

    [TestMethod]
    public void ThreeConsecutiveResourceExhaustions_AbortRemainingCorpus()
    {
        var fileSystem = new MemoryFileSystem();
        fileSystem.AddFile("first.cgf", () => throw new OutOfMemoryException("open"));
        fileSystem.AddFile(
            "second.cgf",
            () => new ThrowAtPositionStream(new MemoryStream(BuildTiming746()), 32, () => new OutOfMemoryException("read")));
        fileSystem.AddFile(
            "third.cgf",
            () => new ThrowAtPositionStream(new MemoryStream(BuildTiming746()), 32, () => new OutOfMemoryException("read")));
        fileSystem.AddFile("fourth.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new CaptureDestination(@"D:\virtual\report.json");

        var exitCode = ChunkScanCommand.Run(
            ["first.cgf", "second.cgf", "third.cgf", "fourth.cgf", "-out", "report.json"],
            new StringWriter(),
            new StringWriter(),
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(destination.Report.Aborted);
        Assert.AreEqual("consecutiveResourceExhaustionLimit", destination.Report.AbortReason);
        Assert.AreEqual(ChunkScanService.MaxConsecutiveResourceExhaustions, destination.Report.ConsecutiveResourceExhaustionAbortThreshold);
        Assert.AreEqual(4, destination.Report.Summary.FilesDiscovered);
        Assert.AreEqual(3, destination.Report.Summary.FilesProcessed);
        Assert.AreEqual(1, destination.Report.Summary.FileOutcomes.ResourceExhaustion);
        Assert.AreEqual(2, destination.Report.Summary.ChunkOutcomes.ResourceExhaustion);
        CollectionAssert.AreEqual(
            new[]
            {
                fileSystem.FullPath("first.cgf"),
                fileSystem.FullPath("second.cgf"),
                fileSystem.FullPath("third.cgf"),
            },
            fileSystem.OpenedPaths.ToArray());
    }

    [TestMethod]
    public void SuccessfulFile_ResetsConsecutiveResourceExhaustionCounter()
    {
        var fileSystem = new MemoryFileSystem();
        foreach (var name in new[] { "one.cgf", "two.cgf", "four.cgf", "five.cgf" })
        {
            fileSystem.AddFile(
                name,
                () => new ThrowAtPositionStream(new MemoryStream(BuildTiming746()), 32, () => new OutOfMemoryException("read")));
        }
        fileSystem.AddFile("three.cgf", () => new MemoryStream(BuildRawUnknown746()));
        fileSystem.AddFile("six.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new CaptureDestination(@"D:\virtual\report.json");

        var exitCode = ChunkScanCommand.Run(
            ["one.cgf", "two.cgf", "three.cgf", "four.cgf", "five.cgf", "six.cgf", "-out", "report.json"],
            new StringWriter(),
            new StringWriter(),
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.IsFalse(destination.Report.Aborted);
        Assert.AreEqual(6, destination.Report.Summary.FilesProcessed);
        Assert.AreEqual(6, fileSystem.OpenedPaths.Count);
    }

    [TestMethod]
    public void MidReadIoFailure_IsOperationalAndLaterFilesContinue()
    {
        var fileSystem = new MemoryFileSystem();
        var firstPath = fileSystem.FullPath("first.cgf");
        fileSystem.AddFile(
            "first.cgf",
            () => new ThrowAtPositionStream(
                new MemoryStream(BuildTiming746()),
                32,
                () => new IOException($"synthetic at {firstPath}")));
        fileSystem.AddFile("second.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new CaptureDestination(@"D:\virtual\report.json");

        var exitCode = ChunkScanCommand.Run(
            ["first.cgf", "second.cgf", "-out", "report.json"],
            new StringWriter(),
            new StringWriter(),
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.IsFalse(destination.Report.Aborted);
        Assert.AreEqual(2, destination.Report.Files.Count);
        Assert.AreEqual(ChunkScanOperationalStage.FileIo, destination.Report.OperationalErrors.Single().Stage);
        var ioFinding = destination.Report.Files[0].Findings.Single();
        Assert.AreEqual(ChunkScanOutcome.IoFailure, ioFinding.Outcome);
        Assert.AreEqual(0, ioFinding.TableIndex);
        Assert.AreEqual(ModelReadStage.ChunkRead, ioFinding.Stage);
        Assert.AreEqual(1, destination.Report.Files[0].ChunkOutcomes.IoFailure);
        using (var ioJson = JsonDocument.Parse(ChunkScanJsonWriter.Serialize(destination.Report)))
            AssertJsonContainsNoStringFragment(ioJson.RootElement, "virtual");
        CollectionAssert.AreEqual(
            new[] { fileSystem.FullPath("first.cgf"), fileSystem.FullPath("second.cgf") },
            fileSystem.OpenedPaths.ToArray());
    }

    [TestMethod]
    public void UnexpectedFileReadFailure_IsFileScopedAndLaterFilesContinue()
    {
        var fileSystem = new MemoryFileSystem();
        fileSystem.AddFile("first.cgf", () => new ThrowOnceOnDisposeStream(new MemoryStream(BuildRawUnknown746())));
        fileSystem.AddFile("second.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new CaptureDestination(@"D:\virtual\report.json");

        var exitCode = ChunkScanCommand.Run(
            ["first.cgf", "second.cgf", "-out", "report.json"],
            new StringWriter(),
            new StringWriter(),
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.AreEqual(2, destination.Report.Summary.FilesProcessed);
        Assert.AreEqual(ChunkScanOperationalStage.Unexpected, destination.Report.OperationalErrors.Single().Stage);
        var finding = destination.Report.Files[0].Findings.Single(item => item.Scope == ChunkScanFindingScope.File);
        Assert.AreEqual(ChunkScanOutcome.UnexpectedFailure, finding.Outcome);
        Assert.AreEqual(ModelReadStage.FileRead, finding.Stage);
        Assert.AreEqual(1, destination.Report.Files[0].FileOutcomes.UnexpectedFailure);
    }

    [TestMethod]
    public void UnexpectedServiceFailure_ReturnsOneInsteadOfCrashing()
    {
        var destination = new CaptureDestination(@"D:\virtual\report.json");
        var error = new StringWriter();

        var exitCode = ChunkScanCommand.Run(
            ["fixture.cgf", "-out", "report.json"],
            new StringWriter(),
            error,
            new UnexpectedDiscoveryFileSystem(),
            _ => destination);

        Assert.AreEqual(1, exitCode);
        StringAssert.Contains(error.ToString(), "unexpected InvalidOperationException");
    }

    [TestMethod]
    public void ReportWriteOutOfMemory_ReturnsOneWithConstantMessage()
    {
        var fileSystem = new MemoryFileSystem();
        fileSystem.AddFile("fixture.cgf", () => new MemoryStream(BuildRawUnknown746()));
        var destination = new ThrowingCommitDestination(
            @"D:\virtual\report.json",
            new OutOfMemoryException("synthetic secret"));
        var error = new StringWriter();

        var exitCode = ChunkScanCommand.Run(
            ["fixture.cgf", "-out", "report.json"],
            new StringWriter(),
            error,
            fileSystem,
            _ => destination);

        Assert.AreEqual(1, exitCode);
        Assert.AreEqual(
            "Failed to write the scan report because memory was exhausted." + Environment.NewLine,
            error.ToString());
    }

    [TestMethod]
    public void RelativePaths_UseDeterministicOwnershipAndGroupingWithoutRootDisclosure()
    {
        var directory = CreateTempDirectory();
        try
        {
            var nested = Path.Combine(directory, "nested");
            Directory.CreateDirectory(nested);
            var topFile = Path.Combine(directory, "top.cgf");
            var nestedFile = Path.Combine(nested, "nested.cgf");
            File.WriteAllBytes(topFile, BuildRawUnknown746());
            File.WriteAllBytes(nestedFile, BuildRawUnknown746());
            var service = new ChunkScanService(new SystemChunkScanFileSystem());
            var inputs = new[] { directory, nested, nestedFile };

            var first = service.Scan(inputs, recursive: true);
            var second = service.Scan(inputs, recursive: true);
            var firstBytes = ChunkScanJsonWriter.Serialize(first);
            var secondBytes = ChunkScanJsonWriter.Serialize(second);

            CollectionAssert.AreEqual(firstBytes, secondBytes);
            var json = Encoding.UTF8.GetString(firstBytes);
            Assert.IsFalse(json.Contains(directory, StringComparison.OrdinalIgnoreCase));
            using var document = JsonDocument.Parse(firstBytes);
            AssertJsonContainsNoStringFragment(document.RootElement, Path.GetFileName(directory));
            Assert.AreEqual(2, first.Files.Count);
            Assert.AreEqual(1, first.Files.Single(file => file.Path == "top.cgf").InputIndex);
            Assert.AreEqual(3, first.Files.Single(file => file.Path == "nested.cgf").InputIndex);
            var group = first.FindingGroups.Single();
            Assert.AreEqual(2, group.OccurrenceCount);
            CollectionAssert.AreEqual(new[] { 1, 2 }, group.AffectedFileIndices.ToArray());
            Assert.AreEqual(2, group.AffectedFileCount);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static int RunCommand(
        string inputPath,
        string reportPath,
        out string output,
        out string error)
    {
        var outputWriter = new StringWriter();
        var errorWriter = new StringWriter();
        var exitCode = ChunkScanCommand.Run(
            [inputPath, "-out", reportPath],
            outputWriter,
            errorWriter,
            new SystemChunkScanFileSystem(),
            ChunkScanJsonWriter.Reserve);
        output = outputWriter.ToString();
        error = errorWriter.ToString();
        return exitCode;
    }

    private static int[] GetExpectedRandomIds(int seed, int count)
    {
        var random = new Random(seed);
        var picked = new HashSet<int>();
        var result = new List<int>(count);
        while (result.Count < count)
        {
            var candidate = random.Next(100000);
            if (picked.Add(candidate))
                result.Add(candidate);
        }
        return result.ToArray();
    }

    private static void AssertJsonContainsNoStringFragment(JsonElement element, string fragment)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    AssertJsonContainsNoStringFragment(property.Value, fragment);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    AssertJsonContainsNoStringFragment(item, fragment);
                break;
            case JsonValueKind.String:
                Assert.IsFalse(
                    (element.GetString() ?? string.Empty).Contains(fragment, StringComparison.OrdinalIgnoreCase),
                    $"JSON string leaked '{fragment}': {element.GetString()}");
                break;
        }
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "cryengine-converter-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static byte[] BuildRawUnknown746() =>
        Build746([(0x1234, 0x2345, 42, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF })]);

    private static byte[] BuildTiming746() =>
        Build746([(0x100E, 0x0918, 44, new byte[48])]);

    private static byte[] BuildSourceInfo746() =>
        Build746([(0x1013, 0x0001, 42, Encoding.ASCII.GetBytes("source\0"))]);

    private static byte[] BuildTimingFailureThenRawUnknown746() =>
        Build746(
            [
                (0x100E, 0x0918, 44, new byte[] { 0x00, 0x00, 0x80, 0x3F }),
                (0x1234, 0x2345, 45, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }),
            ]);

    private static byte[] BuildUnsupportedVersionThenRawUnknown746() =>
        Build746(
            [
                (0x1000, 0x7FFF, 43, Array.Empty<byte>()),
                (0x1234, 0x2345, 44, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }),
            ]);

    private static byte[] BuildTruncatedTable746()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("CrCh"));
        writer.Write((uint)FileVersion.x0746);
        writer.Write(1u);
        writer.Write(16);
        writer.Write((ushort)0x1000);
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] Build746(
        (ushort EncodedType, ushort Version, int Id, byte[] Body)[] chunks)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("CrCh"));
        writer.Write((uint)FileVersion.x0746);
        writer.Write((uint)chunks.Length);
        writer.Write(16);

        var bodyOffset = 16 + chunks.Length * 16;
        foreach (var chunk in chunks)
        {
            writer.Write(chunk.EncodedType);
            writer.Write(chunk.Version);
            writer.Write(chunk.Id);
            writer.Write((uint)chunk.Body.Length);
            writer.Write((uint)bodyOffset);
            bodyOffset += chunk.Body.Length;
        }
        foreach (var chunk in chunks)
            writer.Write(chunk.Body);
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] Build900((uint Type, uint Version, byte[] Body)[] chunks)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("#ivo"));
        writer.Write((uint)FileVersion.x0900);
        writer.Write((uint)chunks.Length);
        writer.Write(16);
        var bodyOffset = 16 + chunks.Length * 16;
        foreach (var chunk in chunks)
        {
            writer.Write(chunk.Type);
            writer.Write(chunk.Version);
            writer.Write((ulong)bodyOffset);
            bodyOffset += chunk.Body.Length;
        }
        foreach (var chunk in chunks)
            writer.Write(chunk.Body);
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildMesh900Body()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(0x01020304);
        writer.Write(2);
        writer.Write(3);
        writer.Write(4u);
        writer.Write(0);
        for (var index = 0; index < 6; index++)
            writer.Write((float)(index + 1));
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildIvoSkinUnknownDataStreamBody(uint typeCode)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0);
        for (var index = 0; index < 12; index++)
            writer.Write(0f);
        writer.Write(0u);
        writer.Write(new byte[92]);
        writer.Write(typeCode);
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] Build744Caf()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("CryTek\0\0"));
        writer.Write((uint)FileType.Animation);
        writer.Write((uint)FileVersion.x0744);
        writer.Write(52);
        writer.Write(999u);
        Write744Entry(writer, 0xDEADBEEFu, 1, 24, 101);
        Write744Entry(writer, 0xFEEDFACEu, 2, 40, 102);
        Write744Entry(writer, 0xDEADBEEFu, 1, 24, 101);
        Write744Entry(writer, 0xFEEDFACEu, 2, 40, 102);
        writer.Flush();
        return stream.ToArray();
    }

    private static void Write744Entry(BinaryWriter writer, uint type, uint version, uint offset, int id)
    {
        writer.Write(type);
        writer.Write(version);
        writer.Write(offset);
        writer.Write(id);
    }

    private static byte[] Build745()
    {
        const uint type = 0xDEADBEEF;
        const uint version = 1;
        const uint offset = 44;
        const int id = 77;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("CryTek\0\0"));
        writer.Write((uint)FileType.Geometry);
        writer.Write((uint)FileVersion.x0745);
        writer.Write(20);
        writer.Write(1u);
        writer.Write(type);
        writer.Write(version);
        writer.Write(offset);
        writer.Write(id);
        writer.Write(16u);
        writer.Write(type);
        writer.Write(version);
        writer.Write(offset);
        writer.Write(id);
        writer.Flush();
        return stream.ToArray();
    }

    private static ChunkScanReport CreateSingleFileReport(ChunkScanFileResult file)
    {
        var report = new ChunkScanReport
        {
            Inputs =
            [
                new ChunkScanInput
                {
                    Index = 1,
                    Alias = "input-1",
                    Kind = ChunkScanInputKind.File,
                },
            ],
            Recursive = false,
            ConsecutiveResourceExhaustionAbortThreshold = ChunkScanService.MaxConsecutiveResourceExhaustions,
            TotalDiscoveredFiles = 1,
        };
        report.Files.Add(file);
        report.FinalizeReport();
        return report;
    }

    private sealed class CaptureDestination(string outputPath) : IChunkScanReportDestination
    {
        public string OutputPath { get; } = outputPath;
        internal ChunkScanReport Report { get; private set; }
        public void Commit(ChunkScanReport report) => Report = report;
        public void Dispose() { }
    }

    private sealed class ThrowingCommitDestination(
        string outputPath,
        Exception exception) : IChunkScanReportDestination
    {
        public string OutputPath { get; } = outputPath;
        public void Commit(ChunkScanReport report) => throw exception;
        public void Dispose() { }
    }

    private sealed class LockedCommitDestination : IChunkScanReportDestination
    {
        private readonly IChunkScanReportDestination inner;
        private readonly FileStream outputLock;

        internal LockedCommitDestination(IChunkScanReportDestination inner, string outputPath)
        {
            this.inner = inner;
            outputLock = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public string OutputPath => inner.OutputPath;
        public void Commit(ChunkScanReport report) => inner.Commit(report);

        public void Dispose()
        {
            outputLock.Dispose();
            inner.Dispose();
        }
    }

    private sealed class ThrowingFileSystem : IChunkScanFileSystem
    {
        private static Exception Unexpected() => new AssertFailedException("The filesystem should not be used.");
        public string GetFullPath(string path) => throw Unexpected();
        public bool FileExists(string path) => throw Unexpected();
        public bool DirectoryExists(string path) => throw Unexpected();
        public FileAttributes GetAttributes(string path) => throw Unexpected();
        public IEnumerable<string> EnumerateFiles(string directory) => throw Unexpected();
        public IEnumerable<string> EnumerateDirectories(string directory) => throw Unexpected();
        public Stream OpenRead(string path) => throw Unexpected();
    }

    private sealed class NoDiscoveryFileSystem : IChunkScanFileSystem
    {
        internal bool DiscoveryTouched { get; private set; }
        public string GetFullPath(string path) => Path.GetFullPath(path);
        public bool FileExists(string path) { DiscoveryTouched = true; throw new AssertFailedException(); }
        public bool DirectoryExists(string path) { DiscoveryTouched = true; throw new AssertFailedException(); }
        public FileAttributes GetAttributes(string path) { DiscoveryTouched = true; throw new AssertFailedException(); }
        public IEnumerable<string> EnumerateFiles(string directory) { DiscoveryTouched = true; throw new AssertFailedException(); }
        public IEnumerable<string> EnumerateDirectories(string directory) { DiscoveryTouched = true; throw new AssertFailedException(); }
        public Stream OpenRead(string path) { DiscoveryTouched = true; throw new AssertFailedException(); }
    }

    private sealed class UnexpectedDiscoveryFileSystem : IChunkScanFileSystem
    {
        public string GetFullPath(string path) => Path.IsPathRooted(path) ? path : Path.Combine(@"D:\virtual", path);
        public bool FileExists(string path) => throw new InvalidOperationException("synthetic unexpected failure");
        public bool DirectoryExists(string path) => false;
        public FileAttributes GetAttributes(string path) => FileAttributes.Normal;
        public IEnumerable<string> EnumerateFiles(string directory) => [];
        public IEnumerable<string> EnumerateDirectories(string directory) => [];
        public Stream OpenRead(string path) => throw new InvalidOperationException();
    }

    private sealed class MemoryFileSystem : IChunkScanFileSystem
    {
        private const string Root = @"D:\virtual";
        private readonly Dictionary<string, Func<Stream>> files = new(StringComparer.OrdinalIgnoreCase);
        internal List<string> OpenedPaths { get; } = [];

        internal string FullPath(string path) => Path.Combine(Root, path);
        internal void AddFile(string path, Func<Stream> factory) => files[FullPath(path)] = factory;
        public string GetFullPath(string path) => Path.IsPathRooted(path) ? path : FullPath(path);
        public bool FileExists(string path) => files.ContainsKey(path);
        public bool DirectoryExists(string path) => string.Equals(path, Root, StringComparison.OrdinalIgnoreCase);
        public FileAttributes GetAttributes(string path) => FileAttributes.Normal;
        public IEnumerable<string> EnumerateFiles(string directory) => files.Keys.Where(path => string.Equals(Path.GetDirectoryName(path), directory, StringComparison.OrdinalIgnoreCase));
        public IEnumerable<string> EnumerateDirectories(string directory) => [];

        public Stream OpenRead(string path)
        {
            OpenedPaths.Add(path);
            return files[path]();
        }
    }

    private class TrackingStream(Stream inner) : Stream
    {
        internal List<(long Offset, SeekOrigin Origin)> Seeks { get; } = [];
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => inner.Read(buffer);
        public override int ReadByte() => inner.ReadByte();
        public override long Seek(long offset, SeekOrigin origin)
        {
            Seeks.Add((offset, origin));
            return inner.Seek(offset, origin);
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();
            base.Dispose(disposing);
        }
    }

    private sealed class ThrowAtPositionStream(
        Stream inner,
        long throwPosition,
        Func<Exception> exceptionFactory) : TrackingStream(inner)
    {
        private bool thrown;

        private void ThrowIfNeeded()
        {
            if (thrown || Position < throwPosition)
                return;
            thrown = true;
            throw exceptionFactory();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfNeeded();
            return base.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            ThrowIfNeeded();
            return base.Read(buffer);
        }

        public override int ReadByte()
        {
            ThrowIfNeeded();
            return base.ReadByte();
        }
    }

    private sealed class ThrowOnceOnCurrentSeekStream(Stream inner) : TrackingStream(inner)
    {
        internal bool Threw { get; private set; }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!Threw && origin == SeekOrigin.Current)
            {
                Threw = true;
                throw new InvalidDataException("synthetic skip failure");
            }
            return base.Seek(offset, origin);
        }
    }

    private sealed class ThrowOnceOnDisposeStream(Stream inner) : TrackingStream(inner)
    {
        private bool thrown;

        protected override void Dispose(bool disposing)
        {
            if (disposing && !thrown)
            {
                thrown = true;
                throw new InvalidOperationException("synthetic dispose failure");
            }
            base.Dispose(disposing);
        }
    }
}
