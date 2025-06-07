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

        Assert.AreEqual((uint)215052963, crc32);
        Assert.AreEqual((uint)0x0CD172A3, crc32);
    }

    [TestMethod]
    public void ComputeCrc32_Hud_ValidateCrC()
    {
        var expected = "hud";
        var crc32b = Crc32CryEngine.Compute(expected);

        Assert.AreEqual((uint)2702338936, crc32b);
        Assert.AreEqual((uint)0xA1126B78, crc32b);
    }

    [TestMethod]
    public void Crc32_CryEngineExample_ShouldPass()
    {
        string input = "123456789";
        uint expectedCrc = 0xCBF43926;

        uint actualCrc = Crc32CryEngine.Compute(input);

        Assert.AreEqual(expectedCrc, actualCrc);
    }

    [TestMethod]
    public void Crc32_Nose()
    {
        string input = "Nose";
        uint expectedCrc = 0x20CEC3ED;  // 550421485

        uint actualCrc = Crc32CryEngine.Compute(input);

        Assert.AreEqual(expectedCrc, actualCrc);
    }
}
