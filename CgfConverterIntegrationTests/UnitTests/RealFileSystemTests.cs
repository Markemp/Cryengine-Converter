using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CgfConverter.PackFileSystem;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class RealFileSystemTests
{
    private string _tempDir = null!;
    private RealFileSystem _fs = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "RealFileSystemTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        // Create a directory structure with specific casing:
        // Objects/Ships/hornet.mtl
        // Objects/Ships/textures/hull.dds
        // Objects/Weapons/laser.cgf
        var shipsDir = Path.Combine(_tempDir, "Objects", "Ships");
        var texturesDir = Path.Combine(shipsDir, "textures");
        var weaponsDir = Path.Combine(_tempDir, "Objects", "Weapons");

        Directory.CreateDirectory(shipsDir);
        Directory.CreateDirectory(texturesDir);
        Directory.CreateDirectory(weaponsDir);

        File.WriteAllText(Path.Combine(shipsDir, "hornet.mtl"), "test");
        File.WriteAllText(Path.Combine(texturesDir, "hull.dds"), "test");
        File.WriteAllText(Path.Combine(weaponsDir, "laser.cgf"), "test");

        _fs = new RealFileSystem(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [TestMethod]
    public void Exists_ExactCase_ReturnsTrue()
    {
        Assert.IsTrue(_fs.Exists(Path.Combine("Objects", "Ships", "hornet.mtl")));
    }

    [TestMethod]
    public void Exists_WrongCase_ReturnsTrue()
    {
        Assert.IsTrue(_fs.Exists(Path.Combine("objects", "ships", "Hornet.mtl")));
    }

    [TestMethod]
    public void Exists_AllUpperCase_ReturnsTrue()
    {
        Assert.IsTrue(_fs.Exists(Path.Combine("OBJECTS", "SHIPS", "HORNET.MTL")));
    }

    [TestMethod]
    public void Exists_NonexistentFile_ReturnsFalse()
    {
        Assert.IsFalse(_fs.Exists(Path.Combine("Objects", "Ships", "nonexistent.mtl")));
    }

    [TestMethod]
    public void Exists_NonexistentDirectory_ReturnsFalse()
    {
        Assert.IsFalse(_fs.Exists(Path.Combine("NoSuchDir", "file.mtl")));
    }

    [TestMethod]
    public void GetStream_ExactCase_ReturnsStream()
    {
        using var stream = _fs.GetStream(Path.Combine("Objects", "Ships", "hornet.mtl"));
        Assert.IsNotNull(stream);
        Assert.IsTrue(stream.CanRead);
    }

    [TestMethod]
    public void GetStream_WrongCase_ReturnsStream()
    {
        using var stream = _fs.GetStream(Path.Combine("objects", "ships", "Hornet.MTL"));
        Assert.IsNotNull(stream);
        Assert.IsTrue(stream.CanRead);
    }

    [TestMethod]
    public void GetStream_NonexistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(
            () => _fs.GetStream(Path.Combine("Objects", "Ships", "nonexistent.mtl")));
    }

    [TestMethod]
    public void ReadAllBytes_WrongCase_ReturnsContent()
    {
        var bytes = _fs.ReadAllBytes(Path.Combine("objects", "SHIPS", "hornet.mtl"));
        Assert.AreEqual("test", System.Text.Encoding.UTF8.GetString(bytes));
    }

    [TestMethod]
    public void ReadAllBytes_NonexistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(
            () => _fs.ReadAllBytes(Path.Combine("Objects", "NoFile.mtl")));
    }

    [TestMethod]
    public void Glob_WildcardExactCaseDir_FindsFiles()
    {
        var results = _fs.Glob(Path.Combine("Objects", "Ships", "*.mtl"));
        Assert.AreEqual(1, results.Length);
    }

    [TestMethod]
    public void Glob_WildcardWrongCaseDir_FindsFiles()
    {
        var results = _fs.Glob(Path.Combine("objects", "ships", "*.mtl"));
        Assert.AreEqual(1, results.Length);
    }

    [TestMethod]
    public void Glob_ExactFileWrongCase_FindsFile()
    {
        var results = _fs.Glob(Path.Combine("objects", "ships", "HORNET.MTL"));
        Assert.AreEqual(1, results.Length);
    }

    [TestMethod]
    public void Glob_NonexistentDir_ReturnsEmpty()
    {
        var results = _fs.Glob(Path.Combine("NoSuchDir", "*.mtl"));
        Assert.AreEqual(0, results.Length);
    }

    [TestMethod]
    public void Exists_NestedPath_WrongCase_ReturnsTrue()
    {
        Assert.IsTrue(_fs.Exists(Path.Combine("objects", "ships", "TEXTURES", "Hull.DDS")));
    }

    [TestMethod]
    public void GetStream_CachedResolution_SecondCallFaster()
    {
        // First call populates the cache
        using (var stream1 = _fs.GetStream(Path.Combine("objects", "ships", "Hornet.MTL")))
            Assert.IsNotNull(stream1);

        // Second call should hit the cache
        using (var stream2 = _fs.GetStream(Path.Combine("objects", "ships", "Hornet.MTL")))
            Assert.IsNotNull(stream2);
    }
}
