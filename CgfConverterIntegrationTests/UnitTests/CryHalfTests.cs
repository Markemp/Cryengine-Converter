using CgfConverter.Utililities;
using NUnit.Framework;

namespace CgfConverterTests.UnitTests;

[TestFixture]
public class CryHalfTests
{
    [Test]
    public void HalfToFloat_Zero_ReturnsZero()
    {
        // Arrange
        ushort half = 0;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(0f, result);

    }

    [Test]
    public void HalfToFloat_MaxValue_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x7FFF;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(1.20795136E+09f, result);
    }

    [Test]
    public void HalfToFloat_a_value_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x4009;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(1.20795136E+09f, result);
    }

    [Test]
    public void HalfToFloat_RandomValue_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x52c2;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        Assert.AreEqual(1.11307981E+09f, result);
    }

    [Test]
    public void HalfToFloat_One_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x3c00;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(1.06535322E+09f, result);
    }

    [Test]
    public void HalfToFloat_Three_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x3e00;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(1.06954752E+09f, result);
    }

    [Test]
    public void HalfToFloat_Ten_ReturnsCorrectResult()
    {
        // Arrange
        ushort half = 0x4400;

        // Act
        float result = CryHalf.ConvertToFloat(half);

        // Assert
        Assert.AreEqual(1.08213043E+09f, result);
    }
}