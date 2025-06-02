using Microsoft.VisualStudio.TestTools.UnitTesting;
using CgfConverter.Utils;
using System.Linq;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("unit")]
public class CryXmlSerializerTests
{
    string objectDir = @"d:\depot\kcd2";

    [TestMethod]
    public void FromFile_SimpleMaterialXmlFile_NoSubmats()
    {
        var filename = @"..\..\..\TestData\SimpleMat.xml";

        var material = MaterialUtilities.FromFile(filename, "material name");

        Assert.IsNotNull(material.SubMaterials);
        Assert.AreEqual(4, material.Textures.Length);
        Assert.AreEqual("material name", material.SubMaterials[0].Name);
    }

    [TestMethod]
    public void FromFile_MutliMaterialXmlFile_HasSubmats()
    {
        var filename = @"..\..\..\TestData\MultipleMats.xml";

        var material = MaterialUtilities.FromFile(filename, null);

        Assert.IsNotNull(material.SubMaterials);
        Assert.IsNull(material.Textures);

        Assert.AreEqual(5, material.SubMaterials.Length);
    }

    [TestMethod]
    public void FromFile_StarCitizenBinaryMatFile()
    {
        var filename = @"..\..\..\TestData\SC_mat.mtl";

        var material = MaterialUtilities.FromFile(filename, null);
        Assert.IsNotNull(material.SubMaterials);
        Assert.AreEqual(2, material.SubMaterials.Length);
    }

    [TestMethod]
    public void Pbxml_DeserializeFile()
    {
        var filename = @"..\..\..\TestData\pbxml.mtl";
        var material = MaterialUtilities.FromFile(filename, null);

        Assert.IsNotNull(material.SubMaterials);
        Assert.AreEqual(2, material.SubMaterials.Length);
        Assert.AreEqual("524544", material.MtlFlags);
    }

    [TestMethod]
    public void FromFile_M_Head_Capon_M01_mtl()
    {
        var filename = $@"{objectDir}\objects\characters\humans\male\head\m_head_capon\m_head_capon_m01.mtl";
        var material = MaterialUtilities.FromFile(filename, "m_head_capon_m01", objectDir);
        Assert.AreEqual(5, material.SubMaterials.Count());
    }

    [TestMethod]
    public void FromFile_clothing_main_m()
    {
        var filename = @"..\..\..\TestData\clothing_main_m.mtl";
        var material = MaterialUtilities.FromFile(filename, "clothing_main_m", objectDir);
        Assert.AreEqual(1, material.SubMaterials.Count());
    }
}
