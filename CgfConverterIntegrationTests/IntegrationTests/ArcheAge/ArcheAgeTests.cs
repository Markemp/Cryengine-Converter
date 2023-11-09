using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
public class ArcheAgeTests
{
    private readonly TestUtils testUtils = new TestUtils();
    string userHome;

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void ArcheAge_ChrFileTest()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\ArcheAge\coupleduckship_foot.chr", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(6, actualMaterialsCount);

        // Controller check
        var controller = daeObject.Library_Controllers.Controller;
        var expectedBones = "Bip01 Locator_Locomotion bone_coupleduckship_fix bone_coupleduckship_pedal_r_01_FirstBone";
        Assert.IsTrue(controller[0].Skin.Source[0].Name_Array.Value_Pre_Parse.StartsWith(expectedBones));
        var expectedBpm = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 -0 0 -0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0 0 1 0 " +
            "0 0 0 1 0 0 0 0 1 0.116992 -0.048410 0.991952 0.418401 -0.916581 0.379274 0.126612 -2.546408";
        Assert.IsTrue(controller[0].Skin.Source[1].Float_Array.Value_As_String.StartsWith(expectedBpm));
        Assert.AreEqual(368, controller[0].Skin.Source[1].Float_Array.Count);

        // Visual Scene Check 
        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature Node check 
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("Bip01", node.ID);
        Assert.AreEqual("Bip01", node.sID);
        Assert.AreEqual("Bip01", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("0 0 0", node.Translate[0].Value_As_String);
        Assert.AreEqual("1 0 0 0", node.Rotate[0].Value_As_String);
        var locatorBone = node.node[0];
        Assert.AreEqual("Locator_Locomotion", locatorBone.ID);
        Assert.AreEqual("Locator_Locomotion", locatorBone.Name);
        Assert.AreEqual("Locator_Locomotion", locatorBone.sID);
        Assert.AreEqual("JOINT", locatorBone.Type.ToString());
        Assert.AreEqual("0 0 0", locatorBone.Translate[0].Value_As_String);
        Assert.AreEqual("1 0 0 0", locatorBone.Rotate[0].Value_As_String);
    }
}
