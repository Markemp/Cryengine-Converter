using CgfConverter.Utililities;
using CgfConverterTests.TestUtilities;
using NUnit.Framework;

namespace CgfConverterTests.UnitTests;

[TestFixture]
public class CryHalfTests
{
    [Test]
    public void CryHalfToFloat_0x4de2_ReturnsCorrectValue()
    {
        ushort value = 0x4DE2;
        float expectedResult = 23.53125f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertCryHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void DymekHalf_0x4de2_ReturnsCorrectValue()
    {
        ushort value = 0x4DE2;
        float expectedResult = 0.6132504940032959f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertDymekHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_0x01D2_ReturnsCorrectValue()
    {
        ushort value = 0x01D2;
        float expectedResult = 0.0000277757644653f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertCryHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_0x01D2_ReturnsCorrectValue()
    {
        ushort value = 0x01D2;
        float expectedResult = 0.014333168976008892f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertDymekHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_ZeroValue_ReturnsCorrectValue()
    {
        ushort value = 0x0000;
        float expectedResult = 0f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertCryHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void DymekHalf_ZeroValue_ReturnsCorrectValue()
    {
        ushort value = 0x0000;
        float expectedResult = 0f;

        Assert.AreEqual(expectedResult, CryHalf.ConvertDymekHalfToFloat(value), TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_MaxValue_ReturnsCorrectResult()
    {
        ushort half = 0x7FFF;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(131008.0f, result, TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_MaxValue_ReturnsCorrectResult()
    {
        ushort half = 0x7FFF;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(1.0078432559967041d, result, TestUtils.delta);
    }


    [Test]
    public void CryHalfToFloat_0004396_ReturnsCorrectResult()
    {
        ushort half = 0x0f34;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(0.0004396, result, TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_ReturnsCorrectResult()
    {
        ushort half = 0x0f34;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.11970964819192886d, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_a_value_ReturnsCorrectResult()
    {
        ushort half = 0x4009;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(2.017578125d, result, TestUtils.delta);
    }
    
    [Test]
    public void DymekHalfToFloat_a_value_ReturnsCorrectResult()
    {
        ushort half = 0x4009;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.50421380996704102d, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_RandomValue_ReturnsCorrectResult()
    {
        ushort half = 0x52c2;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(54.0625f, result, TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_RandomValue_ReturnsCorrectResult()
    {
        ushort half = 0x52c2;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.651636302f, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_One_ReturnsCorrectResult()
    {
        ushort half = 0x3c00;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(1.0f, result, TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_One_ReturnsCorrectResult()
    {
        ushort half = 0x3c00;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.47244095802307129d, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_Three_ReturnsCorrectResult()
    {
        ushort half = 0x3e00;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(1.5f, result, TestUtils.delta);
    }

    [Test]
    public void DymekHalfToFloat_Three_ReturnsCorrectResult()
    {
        ushort half = 0x3e00;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.4881889820098877d, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_Four_ReturnsCorrectResult()
    {
        ushort half = 0x4400;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(4f, result, TestUtils.delta);
    }
    [Test]
    public void DymekHalfToFloat_Four_ReturnsCorrectResult()
    {
        ushort half = 0x4400;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.53543305397033691d, result, TestUtils.delta);
    }

    [Test]
    public void CryHalfToFloat_ReturnsCorrectResult()
    {
        ushort half = 0x0f3f;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.AreEqual(0.00044226646423339844d, result, TestUtils.delta);
    }
    
    [Test]
    public void DymekHalfToFloat_pt120048_ReturnsCorrectResult()
    {
        ushort half = 0x0f3f;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.AreEqual(0.120047979f, result, TestUtils.delta);
    }
}