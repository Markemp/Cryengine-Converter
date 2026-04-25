using Microsoft.VisualStudio.TestTools.UnitTesting;
using CgfConverter.Utils;
using System.Linq;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
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

    [TestMethod]
    public void FromFile_EmptyMatLayersElement_DoesNotFallBackToDefault()
    {
        // Regression: a submaterial with a self-closing <MatLayers/> element used to
        // throw NullReferenceException (Layers is null) which was silently swallowed,
        // causing the whole file to fall back to a single default material and
        // producing empty _materials scopes in rendered output.
        var filename = @"..\..\..\TestData\MatLayersEmpty.xml";

        var material = MaterialUtilities.FromFile(filename, "MatLayersEmpty");

        Assert.IsNotNull(material);
        Assert.IsNotNull(material.SubMaterials, "Parse should not fall back to default (which has no submaterials)");
        Assert.AreEqual(3, material.SubMaterials.Length);

        var emptyLayered = material.SubMaterials.Single(m => m.Name == "plastic_2_matte_black");
        Assert.IsNotNull(emptyLayered.MatLayers, "Empty <MatLayers/> still deserializes to an object");
        Assert.IsNull(emptyLayered.MatLayers.Layers, "Empty element should have null Layers array");
        Assert.IsNull(emptyLayered.SubMaterials, "No layers to flatten, so no sub-layer submaterials");
    }
}
