using CgfConverter;
using CgfConverter.PackFileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace CgfConverterTests.UnitTests;

/// <summary>
/// Regression tests for issue #242: animations that ship only as intermediate
/// <c>.i_caf</c> files (e.g. Pandemic Express no-weapon animations) were silently
/// dropped because the wildcard sweep only matched compiled <c>.caf</c> files.
/// </summary>
[TestClass]
[TestCategory("unit")]
public class IcafAnimationDiscoveryTests
{
    private string _tempRoot = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "cgf_icaf_" + Guid.NewGuid().ToString("N"));
        var animDir = Path.Combine(_tempRoot, "anim", "sub");
        Directory.CreateDirectory(animDir);

        // a: ships as both compiled + intermediate -> compiled wins, intermediate dropped
        File.WriteAllText(Path.Combine(animDir, "a.caf"), "");
        File.WriteAllText(Path.Combine(animDir, "a.i_caf"), "");
        // b: intermediate only -> must be discovered (the issue #242 case)
        File.WriteAllText(Path.Combine(animDir, "b.i_caf"), "");
        // c: compiled only -> discovered as before
        File.WriteAllText(Path.Combine(animDir, "c.caf"), "");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, true);
    }

    [TestMethod]
    public void ExpandCafWildcard_IncludesIcafWithoutCafSibling_AndPrefersCaf()
    {
        var ce = new CryEngine("model.chr", new RealFileSystem(_tempRoot)) { ObjectDir = _tempRoot };

        var results = ce.ExpandCafWildcard(Path.Combine("anim", "*", "*.caf"))
            .Select(Path.GetFileName)
            .ToList();

        CollectionAssert.Contains(results, "c.caf", "compiled-only animation should still be found");
        CollectionAssert.Contains(results, "b.i_caf", "intermediate-only (.i_caf) animation must be discovered");
        CollectionAssert.Contains(results, "a.caf", "compiled file should be kept when both exist");
        CollectionAssert.DoesNotContain(results, "a.i_caf", "intermediate should be dropped when a compiled .caf sibling exists");
    }
}
