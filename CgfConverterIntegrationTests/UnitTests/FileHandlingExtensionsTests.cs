using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Extensions.FileHandlingExtensions;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class FileHandlingExtensionsTests
{
    [TestMethod]
    public void CombineAndNormalizePath_SingleSimplePath_ReturnsSamePath()
    {
        var result = CombineAndNormalizePath("path1");
        Assert.AreEqual("path1", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_MultipleSimplePaths_ReturnsCombinedPath()
    {
        var result = CombineAndNormalizePath("path1", "path2", "path3");
        Assert.AreEqual("path1\\path2\\path3", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_PathsContainingSpaces_ReturnsTrimmedPath()
    {
        var result = CombineAndNormalizePath(" path1 ", " path2 ");
        Assert.AreEqual("path1\\path2", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_PathsContainingPeriodSymbol_ReturnsNormalizedPath()
    {
        var result = CombineAndNormalizePath("path1", ".", "path2");
        Assert.AreEqual("path1\\path2", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_PathsContainingTwoPeriodSymbol_ReturnsNormalizedPath()
    {
        var result = CombineAndNormalizePath("path1", "..", "path2");
        Assert.AreEqual("path2", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_NullOrEmptyPaths_ReturnsEmptyString()
    {
        var result = CombineAndNormalizePath(null, "", " ");
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_MixedSeparators_ReturnsNormalizedPath()
    {
        var result = CombineAndNormalizePath("path1/path2", "path3\\path4");
        Assert.AreEqual("path1\\path2\\path3\\path4", result);
    }

    [TestMethod]
    public void CombineAndNormalizePath_DirAndMaterialFile()
    {
        var result = CombineAndNormalizePath("c:/dir", "material.mtl");
        Assert.AreEqual("c:\\dir\\material.mtl", result);
    }
}
