using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class NewWorldIntegrationTests
{
    private readonly TestUtils testUtils = new();
    private readonly string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly string objectDir = @"d:\depot\newworld";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    [TestMethod]
    public void PrimitiveBox_Collada()
    {
        var args = new string[] { $@"{objectDir}\Objects\default\primitive_box.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, new CryEngineOptions(ObjectDir: objectDir));
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
    }

    [TestMethod]
    public void PrimitiveBox_Collada_WithMatFile()
    {
        var args = new string[] { $@"{objectDir}\Objects\default\primitive_box.cgf" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, new CryEngineOptions("editorprimitive_b_mat.mtl", objectDir));
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        // Verify materials
        var matLibrary = daeObject.Library_Materials;
        Assert.AreEqual(2, matLibrary.Material.Length);
        Assert.AreEqual("editorprimitive_b_mat_mtl_coll-material", matLibrary.Material[0].ID);
        Assert.AreEqual("editorprimitive_b_mat_mtl_mat-material", matLibrary.Material[1].ID);

    }

    [TestMethod]
    public void Npc_Horus_Skel_Chr()
    {
        var args = new string[] { $@"{objectDir}\Objects\characters\npc\npc_horus_skel.chr", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
        Assert.AreEqual(2, nodes.Length);

        Assert.AreEqual("1 0 0 -0 0 1 0 0.022861 0 0 1 -1.073091 0 0 0 1", nodes[0].node[0].Matrix[0].Value_As_String);
        Assert.AreEqual("Pelvis", nodes[0].node[0].Name);
    }

    [TestMethod]
    public void Adiana_Body_Skin()
    {
        var args = new string[] { $@"{objectDir}\objects\characters\npc\natural\adiana\adiana_body.skin", "-dds", "-dae", "-objectdir", objectDir };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
        // Verify materials
        var matLibrary = daeObject.Library_Materials;
        Assert.AreEqual(5, matLibrary.Material.Length);
        Assert.AreEqual("4976a0e4-6fd8-560a-88de-cf687eb6ec9d_mtl_material0-material", matLibrary.Material[0].ID);
        Assert.AreEqual("4976a0e4-6fd8-560a-88de-cf687eb6ec9d_mtl_material1-material", matLibrary.Material[1].ID);
    }

    [TestMethod]
    public void Adiana_Body_Skin_WithMatFile()
    {
        var args = new string[] { $@"{objectDir}\objects\characters\npc\natural\adiana\adiana_body.skin", "-dds", "-dae", "-objectdir", objectDir };

        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem, new CryEngineOptions("hood_mat_matgroup.mtl", objectDir));
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
        // Verify materials
        var matLibrary = daeObject.Library_Materials;
        Assert.AreEqual(5, matLibrary.Material.Length);
        Assert.AreEqual("hood_mat_matgroup_mtl_hood_mat-material", matLibrary.Material[0].ID);
        Assert.AreEqual("hood_mat_matgroup_mtl_leaves_mat-material", matLibrary.Material[1].ID);
    }

    [TestMethod]
    public void AugerTrap_cgf()
    {
        var args = new string[] { $@"{objectDir}\objects\props\augertrap\augertrap.cgf", "-dds", "-dae", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(args[0], testUtils.argsHandler.Args.PackFileSystem);
        cryData.ProcessCryengineFiles();

        var colladaData = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        colladaData.GenerateDaeObject();
        var daeObject = colladaData.DaeObject;
        var nodes = daeObject.Library_Visual_Scene.Visual_Scene[0].Node;
    }

    [TestMethod]
    public void ChairSittingIdle_CAF_Controller831_ParsesCorrectly()
    {
        var cafPath = $@"{objectDir}\animations\gameplay\character\player\male\roleplay\chair\roleply_chair_sitting_idle.caf";
        var model = Model.FromStream(cafPath, File.OpenRead(cafPath), closeStream: true);

        var controllers = model.ChunkMap.Values.OfType<ChunkController_831>().ToList();
        Assert.IsTrue(controllers.Count > 0, "Should have ChunkController_831 chunks");

        foreach (var ctrl in controllers)
        {
            // Verify quaternions are approximately normalized (length ~1.0)
            // Compressed formats (SmallTree48Bit, SmallTree64BitExt) may have minor precision loss
            foreach (var rot in ctrl.KeyRotations)
            {
                var length = rot.Length();
                Assert.IsTrue(length > 0.9f && length < 1.1f,
                    $"Controller {ctrl.ControllerId:X}: rotation not normalized, length={length}");
            }

            // Verify positions are finite and reasonable
            foreach (var pos in ctrl.KeyPositions)
            {
                Assert.IsFalse(float.IsNaN(pos.X) || float.IsNaN(pos.Y) || float.IsNaN(pos.Z),
                    $"Controller {ctrl.ControllerId:X}: NaN in position data");
                Assert.IsTrue(pos.Length() < 1000f,
                    $"Controller {ctrl.ControllerId:X}: position magnitude unreasonable ({pos.Length()})");
            }

            // Key times should be monotonically non-decreasing
            for (int i = 1; i < ctrl.RotationKeyTimes.Count; i++)
            {
                Assert.IsTrue(ctrl.RotationKeyTimes[i] >= ctrl.RotationKeyTimes[i - 1],
                    $"Controller {ctrl.ControllerId:X}: rotation key times not monotonic at index {i}");
            }

            for (int i = 1; i < ctrl.PositionKeyTimes.Count; i++)
            {
                Assert.IsTrue(ctrl.PositionKeyTimes[i] >= ctrl.PositionKeyTimes[i - 1],
                    $"Controller {ctrl.ControllerId:X}: position key times not monotonic at index {i}");
            }
        }
    }

    [TestMethod]
    public void PlayerMale_Chr_LoadsSkeleton()
    {
        var chrPath = $@"{objectDir}\objects\characters\player\male\player_male.chr";
        var args = new string[] { chrPath, "-usd", "-objectdir", objectDir };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(chrPath, testUtils.argsHandler.Args.PackFileSystem,
            new CryEngineOptions(ObjectDir: objectDir));
        cryData.ProcessCryengineFiles();

        // Verify skeleton loaded
        Assert.IsNotNull(cryData.SkinningInfo?.CompiledBones, "Skeleton should be loaded");
        Assert.IsTrue(cryData.SkinningInfo.CompiledBones.Count > 0, "Should have bones");
    }

}
