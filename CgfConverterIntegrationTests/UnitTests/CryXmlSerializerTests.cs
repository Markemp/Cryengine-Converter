using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CgfConverter.Utils;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("unit")]
public class CryXmlSerializerTests
{
    [TestMethod]
    public void ReadFile_SimpleMaterialXmlFile_NoSubmats()
    {
        var filename = @"..\..\..\TestData\SimpleMat.xml";

        var material = MaterialUtilities.FromFile(filename, "material name");

        Assert.IsNotNull(material.SubMaterials);
        Assert.AreEqual(4, material.Textures.Length);
        Assert.AreEqual("material name", material.SubMaterials[0].Name);
    }

    [TestMethod]
    public void ReadFile_MutliMaterialXmlFile_HasSubmats()
    {
        var filename = @"..\..\..\TestData\MultipleMats.xml";

        var material = MaterialUtilities.FromFile(filename, null);

        Assert.IsNotNull(material.SubMaterials);
        Assert.IsNull(material.Textures);

        Assert.AreEqual(5, material.SubMaterials.Length);
    }

    [TestMethod]
    public void ReadFile_StarCitizenBinaryMatFile()
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
}
