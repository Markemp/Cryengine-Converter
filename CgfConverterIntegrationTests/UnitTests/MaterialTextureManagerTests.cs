using CgfConverter;
using CgfConverter.Models.Materials;
using CgfConverter.Renderers.MaterialTextures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.UnitTests;

[TestClass]
[TestCategory("unit")]
public class MaterialTextureManagerTests
{
    [TestMethod]
    public void CreateSet_SubmaterialWithoutTextures_ReturnsEmptySet()
    {
        // A submaterial without a <Textures> element (Star Citizen's AEGS
        // Vanguard paint .mtl) deserializes with Textures == null.
        var manager = new MaterialTextureManager(new Args());
        var material = new Material { Name = "untextured", Textures = null };

        var set = manager.CreateSet(material);

        Assert.IsNull(set.Diffuse);
        Assert.IsNull(set.Normal);
        Assert.IsNull(set.Specular);
    }
}
