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
    // Archeage tests have the objectdir at the game directory level instead of object dir.  So although the
    // game files are under d:\depot\archeage\game, the object dir is d:\depot\archeage.  The material files
    // are referencing the game directory.
    private readonly TestUtils testUtils = new();
    private readonly string objectDir = @"d:\depot\archeage";
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
        var args = new string[]
        {
            @"d:\depot\archeage\game\objects\characters\animals\bird\bird_a.chr",
            "-dds",
            "-obj", objectDir,
            "-mtl", "bird_a.mtl"
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem, materialFile: args[5]);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(2, actualMaterialsCount);

        // Controller check
        var controller = daeObject.Library_Controllers.Controller;
        var expectedBones = "Bone04 Bone05 Bone01 Bone06 Bone06(mirrored) Bone02 Bone07 Bone08 Bone07(mirrored) Bone08(mirrored)";
        Assert.IsTrue(controller[0].Skin.Source[0].Name_Array.Value_Pre_Parse.StartsWith(expectedBones));
        var expectedBpm = "1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 1 0 0 0.100000 0 1 0 0 0 0 1 0 0 0 0 1 -0 0 -1 -0 -0.999108 0.042216 0 -0.071707 0.042216 0.999108 0 0.018175 0 0 0 1 0.967510 -0.183784 -0.173634 0.007351 0.186228 0.982504 -0.002255 -0.008423 0.171010 -0.030154 0.984808 0.026689";
        Assert.IsTrue(controller[0].Skin.Source[1].Float_Array.Value_As_String.StartsWith(expectedBpm));
        Assert.AreEqual(160, controller[0].Skin.Source[1].Float_Array.Count);

        // Visual Scene Check 
        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature Node check 
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("Bone04", node.ID);
        Assert.AreEqual("Bone04", node.sID);
        Assert.AreEqual("Bone04", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);
        var locatorBone = node.node[0];
        Assert.AreEqual("Bone05", locatorBone.ID);
        Assert.AreEqual("Bone05", locatorBone.Name);
        Assert.AreEqual("Bone05", locatorBone.sID);
        Assert.AreEqual("JOINT", locatorBone.Type.ToString());
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", locatorBone.Matrix[0].Value_As_String);
    }

    [TestMethod]
    public void ArcheAge_ChrFileTest_VerifyMaterials()
    {
        var args = new string[]
        {
            @"d:\depot\archeage\game\objects\characters\animals\bird\bird_a.chr",
            "-dds",
            "-obj", objectDir
        };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(2, actualMaterialsCount);
    }

    [TestMethod]
    public void DrugBoy_Chr()
    {
        var args = new string[]
        {
            @"D:\depot\archeage\game\objects\characters\people\drug_boy01\face\drug_boy01_face01\drug_boy01_face01.chr",
            "-dds",
            "-obj", objectDir,
        };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        var cryData = new CryEngine(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();
    }
}
