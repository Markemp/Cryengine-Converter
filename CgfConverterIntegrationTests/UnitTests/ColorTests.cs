using CgfConverter.Models.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class ColorTests
{
    [TestMethod]
    public void Deserialize_ValidInput_ReturnsColor()
    {
        var expected = new Color { Blue = 0.5f, Green = 0.5f, Red = 0.5f };

        var input = "0.5,0.5,0.5";
        var result = Color.Deserialize(input);

        Assert.AreEqual(expected.Red, result.Red);
        Assert.AreEqual(expected.Green, result.Green);
        Assert.AreEqual(expected.Blue, result.Blue);
    }

    [TestMethod]
    public void Deserialize_OnlyTwoValues_ReturnsNull()
    {
        var input = "0.5,0.5";
        var result = Color.Deserialize(input);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Serialize_ValidColorWithDefaultSeparator_ReturnsCorrectString()
    {
        var actual = new Color() { Red = 0.5f, Green = 0.5f, Blue = 0.5f };
        var expected = "0.5 0.5 0.5";

        Assert.AreEqual(expected, Color.Serialize(actual));
    }

    [TestMethod]
    public void Serialize_ValidColorWithCommaSeparator_ReturnsCorrectString()
    {
        var actual = new Color() { Red = 0.5f, Green = 0.5f, Blue = 0.5f };
        var expected = "0.5,0.5,0.5";

        Assert.AreEqual(expected, Color.Serialize(actual, ','));
    }

    [TestMethod]
    public void ToString_ReturnsCorrectlyFormattedString()
    {
        var expected = "R: 0.5, G: 0.5, B: 0.5";
        var actual = new Color() { Red = 0.5f, Green = 0.5f, Blue = 0.5f };

        Assert.AreEqual(expected, actual.ToString());
    }
}
