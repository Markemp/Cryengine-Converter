using CgfConverter.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CgfConverter.Utilities.HashingExtensions;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class HashingExtensionTests
{
    [TestMethod]
    public void ComputeCrc32_Body_ValidateCrC()
    {
        var expected = "body";
        var crc32 = expected.ComputeCrc32();

        Assert.AreEqual(0xB20BA8DB, crc32);
    }

    [TestMethod]
    public void ComputeCrc32_Display_ValidateCrC()
    {
        var expected = "display";
        var crc32 = Crc32CryEngine.Compute(expected);

        Assert.AreEqual((uint)0x786B12A1, crc32);
    }

    [TestMethod]
    public void ComputeCrc32_Hud_ValidateCrC()
    {
        var expected = "hud";
        var crc32b = Crc32CryEngine.Compute(expected);

        Assert.AreEqual((uint)0x0CD172A3, crc32b);
    }

    [TestMethod]
    public void Crc32_CryEngineExample_ShouldPass()
    {
        // Arrange
        string input = "123456789";
        uint expectedCrc = 0xCBF43926;

        // Act
        uint actualCrc = Crc32CryEngine.Compute(input);

        // Assert
        Assert.AreEqual(expectedCrc, actualCrc);
    }
}
