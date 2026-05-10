using System;
using System.IO;
using System.Linq;
using CgfConverter.Diagnostics.ChunkScanning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class UnknownChunkScannerTests
{
    [TestMethod]
    public void IsSupportedModelExtension_IncludesSupportedExtensionsCaseInsensitively()
    {
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.CGF"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.cga"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.chr"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.skin"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.anim"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.soc"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.caf"));
        Assert.IsTrue(UnknownChunkScanner.IsSupportedModelExtension("ship.dba"));
    }

    [TestMethod]
    public void IsSupportedModelExtension_ExcludesUnsupportedExtensions()
    {
        Assert.IsFalse(UnknownChunkScanner.IsSupportedModelExtension("texture.dds"));
        Assert.IsFalse(UnknownChunkScanner.IsSupportedModelExtension("material.mtl"));
        Assert.IsFalse(UnknownChunkScanner.IsSupportedModelExtension("entity.xml"));
        Assert.IsFalse(UnknownChunkScanner.IsSupportedModelExtension("leveldata.xml"));
    }

    [TestMethod]
    public void EnumerateInputs_DirectFile_IncludesSupportedFile()
    {
        using var temp = TempDirectory.Create();
        var file = temp.WriteFile("ship.cgf", []);

        var result = new UnknownChunkScanner().EnumerateInputs(new ChunkScanOptions
        {
            Inputs = { file }
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(Path.GetFullPath(file), result[0]);
    }

    [TestMethod]
    public void EnumerateInputs_DirectoryWithoutRecursive_FindsOnlyTopLevelSupportedFiles()
    {
        using var temp = TempDirectory.Create();
        var top = temp.WriteFile("ship.cgf", []);
        temp.WriteFile("texture.dds", []);
        temp.WriteFile(Path.Combine("nested", "nested.cgf"), []);

        var result = new UnknownChunkScanner().EnumerateInputs(new ChunkScanOptions
        {
            Inputs = { temp.Path }
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(Path.GetFullPath(top), result[0]);
    }

    [TestMethod]
    public void EnumerateInputs_DirectoryWithRecursive_FindsNestedSupportedFiles()
    {
        using var temp = TempDirectory.Create();
        var top = temp.WriteFile("ship.cgf", []);
        var nested = temp.WriteFile(Path.Combine("nested", "nested.skin"), []);

        var result = new UnknownChunkScanner().EnumerateInputs(new ChunkScanOptions
        {
            Inputs = { temp.Path },
            Recursive = true
        });

        CollectionAssert.AreEquivalent(
            new[] { Path.GetFullPath(top), Path.GetFullPath(nested) },
            result.ToArray());
    }

    [TestMethod]
    public void EnumerateInputs_DuplicatePaths_DeDuplicates()
    {
        using var temp = TempDirectory.Create();
        var file = temp.WriteFile("ship.cgf", []);

        var result = new UnknownChunkScanner().EnumerateInputs(new ChunkScanOptions
        {
            Inputs = { file, file }
        });

        Assert.AreEqual(1, result.Count);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        private TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public static TempDirectory Create() => new();

        public string WriteFile(string relativePath, byte[] content)
        {
            var path = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, content);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }
}
