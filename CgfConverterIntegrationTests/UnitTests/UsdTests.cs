using CgfConverter;
using CgfConverter.Renderers.USD;
using CgfConverter.Renderers.USD.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class UsdTests
{
    private ArgsHandler args = new();
    private CryEngine cryData;
    private UsdRenderer renderer;

    [TestMethod]
    public void UsdSerialization_CreatesHeaderAndRootXform()
    {
        var usd = new UsdDoc
        {
            Header = new UsdHeader()
        };
        usd.Prims.Add(new UsdXform("root", "/root"));

    }
}
