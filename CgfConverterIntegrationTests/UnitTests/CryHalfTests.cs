using CgfConverter.Utililities;
using CgfConverterTests.TestUtilities;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace CgfConverterTests.UnitTests;

[TestFixture]
[Category("unit")]
public class CryHalfTests
{
    // CryHalf Max value: 131008, min value: -130944
    // Dymek Half max value: 1.0078433, min value: -1.007874
    // Half max value: 65500, min value: -65500

    [Test]
    public void CryHalfToFloat_0x4de2_ReturnsCorrectValue()
    {
        ushort value = 0x4DE2;
        float expectedResult = 23.53125f;

        Assert.That(CryHalf.ConvertCryHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalf_0x4de2_ReturnsCorrectValue()
    {
        ushort value = 0x4DE2;
        float expectedResult = 0.6132504940032959f;

        Assert.That(CryHalf.ConvertDymekHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_0x01D2_ReturnsCorrectValue()
    {
        ushort value = 0x01D2;
        float expectedResult = 0.0000277757644653f;

        Assert.That(CryHalf.ConvertCryHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_0x01D2_ReturnsCorrectValue()
    {
        ushort value = 0x01D2;
        float expectedResult = 0.014333168976008892f;

        Assert.That(CryHalf.ConvertDymekHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_ZeroValue_ReturnsCorrectValue()
    {
        ushort value = 0x0000;
        float expectedResult = 0f;

        Assert.That(CryHalf.ConvertCryHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalf_ZeroValue_ReturnsCorrectValue()
    {
        ushort value = 0x0000;
        float expectedResult = 0f;

        Assert.That(CryHalf.ConvertDymekHalfToFloat(value), Is.EqualTo(expectedResult).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_MaxValue_ReturnsCorrectResult()
    {
        ushort half = 0x7FFF;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(131008.0f).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_MaxValue_ReturnsCorrectResult()
    {
        ushort half = 0x7FFF;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(1.0078432559967041d).Within(TestUtils.delta));
    }


    [Test]
    public void CryHalfToFloat_0004396_ReturnsCorrectResult()
    {
        ushort half = 0x0f34;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.0004396).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_ReturnsCorrectResult()
    {
        ushort half = 0x0f34;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.11970964819192886d).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_a_value_ReturnsCorrectResult()
    {
        ushort half = 0x4009;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(2.017578125d).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_a_value_ReturnsCorrectResult()
    {
        ushort half = 0x4009;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.50421380996704102d).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_RandomValue_ReturnsCorrectResult()
    {
        ushort half = 0x52c2;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(54.0625f).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_RandomValue_ReturnsCorrectResult()
    {
        ushort half = 0x52c2;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.651636302f).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_One_ReturnsCorrectResult()
    {
        ushort half = 0x3c00;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(1.0f).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_One_ReturnsCorrectResult()
    {
        ushort half = 0x3c00;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.47244095802307129d).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_Three_ReturnsCorrectResult()
    {
        ushort half = 0x3e00;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(1.5f).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_Three_ReturnsCorrectResult()
    {
        ushort half = 0x3e00;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.4881889820098877d).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_Four_ReturnsCorrectResult()
    {
        ushort half = 0x4400;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(4f).Within(TestUtils.delta));
    }
    [Test]
    public void DymekHalfToFloat_Four_ReturnsCorrectResult()
    {
        ushort half = 0x4400;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.53543305397033691d).Within(TestUtils.delta));
    }

    [Test]
    public void CryHalfToFloat_ReturnsCorrectResult()
    {
        ushort half = 0x0f3f;

        float result = CryHalf.ConvertCryHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.00044226646423339844d).Within(TestUtils.delta));
    }

    [Test]
    public void DymekHalfToFloat_pt120048_ReturnsCorrectResult()
    {
        ushort half = 0x0f3f;

        float result = CryHalf.ConvertDymekHalfToFloat(half);

        Assert.That(result, Is.EqualTo(0.120047979f).Within(TestUtils.delta));
    }
}
