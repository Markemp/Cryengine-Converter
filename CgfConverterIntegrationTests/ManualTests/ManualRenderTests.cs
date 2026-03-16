using CgfConverter;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.USD;
using CgfConverter.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.ManualTests;

/// <summary>
/// Manual render tests that write output files to the source file's directory.
/// Run these from Test Explorer to quickly iterate on renderer changes.
///
/// These tests are NOT for CI - they require game assets and write to disk.
/// Use TestCategory "manual" to exclude from automated runs.
/// </summary>
[TestClass]
[TestCategory("manual")]
public class ManualRenderTests
{
    private readonly ArgsHandler argsHandler = new();
    private Args args => argsHandler.Args;
    private readonly string armedWarfareObjectDir = @"d:\depot\armoredwarfare";
    private readonly string kcd2ObjectDir = @"d:\depot\kcd2";
    private readonly string mwoObjectDir = @"d:\depot\mwo";
    private readonly string sc41ObjectDir = @"d:\depot\sc4.1\data";
    private readonly string sc44ObjectDir = @"d:\depot\sc4.4\data";
    private readonly string sc46ObjectDir = @"d:\depot\sc4.6\data";
    private readonly string archeageObjectDir = @"d:\depot\archeage";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;

        // Enable debug logging for manual tests
        HelperMethods.LogLevel = LogLevelEnum.Debug;
    }

    #region ArcheAge tests

    [TestMethod]
    public void Basket_Mix_Ani_USD() // has 2 material libraries
    {
        RenderToUsd($@"{archeageObjectDir}\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga", archeageObjectDir);
    }

    [TestMethod]
    public void Basket_Mix_Ani_Collada() // has 2 material libraries
    {
        RenderToCollada($@"{archeageObjectDir}\game\objects\env\01_nuia\001_housing\01_tools\basket_mix_ani.cga", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Chicken_USD() // 0x801 compiled bones format
    {
        RenderToUsd($@"{archeageObjectDir}\game\objects\characters\animals\chicken\chicken.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Chicken_Gltf() // 0x801 compiled bones format
    {
        RenderToGltf($@"{archeageObjectDir}\game\objects\characters\animals\chicken\chicken.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Bird_Collada()
    {
        RenderToCollada($@"{archeageObjectDir}\game\objects\characters\animals\bird\bird_a.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Bird_Gltf()
    {
        RenderToGltf($@"{archeageObjectDir}\game\objects\characters\animals\bird\bird_a.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Bird_USD()
    {
        RenderToUsd($@"{archeageObjectDir}\game\objects\characters\animals\bird\bird_a.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Fishboat_Gltf() // 0x828 controller edge case
    {
        RenderToGltf($@"{archeageObjectDir}\game\objects\env\06_unit\01_ship\fishboat\fishboat.chr", archeageObjectDir);
    }

    [TestMethod]
    public void ArcheAge_Fishboat_USD() // 0x828 controller edge case
    {
        RenderToUsd($@"{archeageObjectDir}\game\objects\env\06_unit\01_ship\fishboat\fishboat.chr", archeageObjectDir);
    }

    #endregion

    #region Armored Warfare Test Files

    [TestMethod]
    public void ArmoredWarfare_Chicken_USD()
    {
        RenderToUsd($@"{armedWarfareObjectDir}\Objects\characters\animals\birds\chicken\chicken.chr", armedWarfareObjectDir);
    }

    [TestMethod]
    public void ArmoredWarfare_Chicken_Gltf()
    {
        RenderToGltf($@"{armedWarfareObjectDir}\Objects\characters\animals\birds\chicken\chicken.chr", armedWarfareObjectDir);
    }

    #endregion

    #region KCD2 Test Files

    [TestMethod]
    public void Kcd2_Boar_Usd()
    {
        RenderToUsd($@"{kcd2ObjectDir}\Objects\characters\animals\boar\boar.chr", kcd2ObjectDir);
    }

    [TestMethod]
    public void Kcd2_Skeleton_Pig_USD()
    {
        RenderToUsd($@"{kcd2ObjectDir}\Objects\characters\animals\boar\skeleton_pig_01.chr", kcd2ObjectDir);
    }

    [TestMethod]
    public void Kcd2_Boar_Gltf()
    {
        RenderToGltf($@"{kcd2ObjectDir}\Objects\characters\animals\boar\boar.chr", kcd2ObjectDir);
    }

    [TestMethod]
    public void Kcd2_Skeleton_Pig_Gltf()
    {
        RenderToGltf($@"{kcd2ObjectDir}\Objects\characters\animals\boar\skeleton_pig_01.chr", kcd2ObjectDir);
    }

    #endregion

    #region MWO Test Files

    [TestMethod]
    public void _50Cal_Necklace_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\Objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", mwoObjectDir);
    }

    [TestMethod]
    public void _50Cal_Necklace_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\Objects\purchasable\cockpit_hanging\50calnecklace\50calnecklace_a.chr", mwoObjectDir);
    }

    [TestMethod]
    public void HulaGirl_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\Objects\purchasable\cockpit_standing\hulagirl\hulagirl_a.cga", mwoObjectDir);
    }

    [TestMethod]
    public void HulaGirl_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\Objects\purchasable\cockpit_standing\hulagirl\hulagirl_a.cga", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Box_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\Objects\default\box.cgf", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderCockpit_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\adder\cockpit_standard\adder_a_cockpit_standard.cga", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderChr_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\mechs\adder\body\adder.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderChr_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\objects\mechs\adder\body\adder.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_AdderChr_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\objects\mechs\adder\body\adder.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Mechanic_Gtlf()
    {
        RenderToGltf($@"{mwoObjectDir}\objects\characters\mechanic\mechanic.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Pilot_Gtlf()
    {
        RenderToGltf($@"{mwoObjectDir}\objects\characters\pilot\pilot.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Pilot_Diagnostic()
    {
        var inputFile = $@"{mwoObjectDir}\objects\characters\pilot\pilot.chr";
        var args = new string[] { inputFile, "-gltf", "-objectdir", mwoObjectDir };
        argsHandler.ProcessArgs(args);

        var cryData = new CryEngine(inputFile, argsHandler.Args.PackFileSystem, new CryEngineOptions(ObjectDir: mwoObjectDir));
        cryData.ProcessCryengineFiles();

        var rootNode = cryData.RootNode;
        var meshData = rootNode?.MeshData;
        var geomInfo = meshData?.GeometryInfo;
        var skinningInfo = cryData.SkinningInfo;

        System.Console.WriteLine($"=== PILOT.CHR DIAGNOSTIC ===");
        System.Console.WriteLine($"RootNode: {rootNode?.Name}");

        if (geomInfo != null)
        {
            System.Console.WriteLine($"\n--- Geometry Info ---");
            System.Console.WriteLine($"Vertices count: {geomInfo.Vertices?.Data.Length ?? 0}");
            System.Console.WriteLine($"Indices count: {geomInfo.Indices?.Data.Length ?? 0}");
            System.Console.WriteLine($"Normals count: {geomInfo.Normals?.Data.Length ?? 0}");
            System.Console.WriteLine($"UVs count: {geomInfo.UVs?.Data.Length ?? 0}");

            var subsets = geomInfo.GeometrySubsets;
            System.Console.WriteLine($"\n--- Geometry Subsets ({subsets?.Count ?? 0}) ---");
            int totalSubsetVerts = 0;
            if (subsets != null)
            {
                foreach (var (subset, idx) in subsets.Select((s, i) => (s, i)))
                {
                    System.Console.WriteLine($"  Subset {idx}: FirstVertex={subset.FirstVertex}, NumVertices={subset.NumVertices}, FirstIndex={subset.FirstIndex}, NumIndices={subset.NumIndices}");
                    totalSubsetVerts += subset.NumVertices;
                }
            }
            System.Console.WriteLine($"Sum of NumVertices: {totalSubsetVerts}");
        }

        if (skinningInfo != null)
        {
            System.Console.WriteLine($"\n--- Skinning Info ---");
            System.Console.WriteLine($"HasSkinningInfo: {skinningInfo.HasSkinningInfo}");
            System.Console.WriteLine($"BoneMappings count: {skinningInfo.BoneMappings?.Count ?? 0}");
            System.Console.WriteLine($"IntVertices count: {skinningInfo.IntVertices?.Count ?? 0}");
            System.Console.WriteLine($"Ext2IntMap count: {skinningInfo.Ext2IntMap?.Count ?? 0}");
            System.Console.WriteLine($"HasIntToExtMapping: {skinningInfo.HasIntToExtMapping}");
            System.Console.WriteLine($"CompiledBones count: {skinningInfo.CompiledBones?.Count ?? 0}");
        }

        // Check for index out of bounds
        if (geomInfo?.Indices != null && geomInfo?.Vertices != null)
        {
            var maxIndex = geomInfo.Indices.Data.Max();
            var vertCount = geomInfo.Vertices.Data.Length;
            System.Console.WriteLine($"\n--- Index Bounds Check ---");
            System.Console.WriteLine($"Max index value: {maxIndex}");
            System.Console.WriteLine($"Vertex count: {vertCount}");
            System.Console.WriteLine($"Index within bounds: {maxIndex < vertCount}");
        }
    }

    [TestMethod]
    public void MWO_Pilot_Usd()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\characters\pilot\pilot.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Turret_Collada()
    {
        RenderToCollada($@"{mwoObjectDir}\objects\gamemodes\turret\turret_a.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Turret_USD()
    {
        RenderToUsd($@"{mwoObjectDir}\objects\gamemodes\turret\turret_a.chr", mwoObjectDir);
    }

    [TestMethod]
    public void MWO_Turret_Gltf()
    {
        RenderToGltf($@"{mwoObjectDir}\objects\gamemodes\turret\turret_a.chr", mwoObjectDir);
    }

    #endregion

    #region SC test files

    [TestMethod]
    public void SC41_Avenger_USD()
    {
        RenderToUsd($@"{sc41ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void SC41_Avenger_Gltf()
    {
        RenderToGltf($@"{sc41ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void SC46_Avenger_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc46ObjectDir, includeAnimations: false);
    }

    [TestMethod]
    public void SC46_Avenger_Gltf()
    {
        RenderToGltf($@"{sc46ObjectDir}\Objects\Spaceships\Ships\AEGS\Avenger\AEGS_Avenger.cga", sc46ObjectDir);
    }

    [TestMethod]
    public void Aloprat_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Characters\Creatures\aloprat\aloprat_skel.chr", sc46ObjectDir);
    }

    [TestMethod]
    public void Aloprat_Skin_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Characters\Creatures\aloprat\aloprat.skin", sc46ObjectDir);
    }

    [TestMethod]
    public void BEHR_LaserCannon_S2_Usd()
    {
        RenderToUsd($@"{sc46ObjectDir}\objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga", sc46ObjectDir);
    }

    [TestMethod]
    public void BEHR_LaserCannon_S2_gltf()
    {
        RenderToGltf($@"{sc41ObjectDir}\objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void BEHR_LaserCannon_S2_Collada()
    {
        RenderToCollada($@"{sc41ObjectDir}\objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void brfl_fps_behr_p4ar_chr_Usd()
    {
        RenderToUsd($@"{sc41ObjectDir}\Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar.chr", sc41ObjectDir);
    }

    [TestMethod]
    public void GLSN_Shiv_Door_Ramp_Cga_USD()
    {
        RenderToUsd($@"{sc41ObjectDir}\objects\spaceships\ships\GLSN\shiv\Maelstrom\GLSN_Shiv_Door_Ramp.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void mgzn_s04_behr_40gb_01_spin_01_USD()
    {
        RenderToUsd($@"{sc41ObjectDir}\objects\fps_weapons\attachments\magazines\behr\mgzn_s04_behr_40gb_01.cga", sc41ObjectDir);
    }

    [TestMethod]
    public void Teapot_USD()
    {
        RenderToUsd($@"{sc41ObjectDir}\objects\default\teapot.cgf", sc41ObjectDir);
    }

    [TestMethod]
    public void AEGS_Avenger_LandingGear_Back_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back_CHR.chr", sc41ObjectDir);
    }

    [TestMethod]
    public void AEGS_Avenger_LandingGear_Back_Gltf()
    {
        RenderToGltf($@"{sc46ObjectDir}\Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back_CHR.chr", sc41ObjectDir);
    }

    [TestMethod]
    public void SC46_Gladius_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Spaceships\Ships\AEGS\Gladius\AEGS_Gladius.cga", sc46ObjectDir);
    }

    [TestMethod]
    public void SC46_Buccaneer_USD()
    {
        RenderToUsd($@"{sc46ObjectDir}\Objects\Spaceships\Ships\DRAK\Buccaneer\Exterior\DRAK_Buccaneer.cga", sc46ObjectDir, includeAnimations: false);
    }
    #endregion

    #region Helper Methods

    private void RenderToUsd(string inputFile, string objectDir, bool includeAnimations = true)
    {
        var cliArgs = new string[] { inputFile, "-usd", "-objectdir", objectDir };
        argsHandler.ProcessArgs(cliArgs);

        var cryData = new CryEngine(inputFile, args.PackFileSystem, new CryEngineOptions(ObjectDir: objectDir, IncludeAnimations: includeAnimations));
        cryData.ProcessCryengineFiles();

        var renderer = new UsdRenderer(args, cryData);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".usda");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    private void RenderToCollada(string inputFile, string objectDir)
    {
        var cliArgs = new string[] { inputFile, "-dae", "-objectdir", objectDir };
        argsHandler.ProcessArgs(cliArgs);

        var cryData = new CryEngine(inputFile, args.PackFileSystem, new CryEngineOptions(ObjectDir: objectDir));
        cryData.ProcessCryengineFiles();

        var renderer = new ColladaModelRenderer(args, cryData);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".dae");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    private void RenderToGltf(string inputFile, string objectDir)
    {
        var cliArgs = new string[] { inputFile, "-gltf", "-objectdir", objectDir };
        argsHandler.ProcessArgs(cliArgs);

        var cryData = new CryEngine(inputFile, args.PackFileSystem, new CryEngineOptions(ObjectDir: objectDir, IncludeAnimations: true));
        cryData.ProcessCryengineFiles();

        var renderer = new GltfModelRenderer(args, cryData);
        renderer.Render();

        var outputPath = Path.ChangeExtension(inputFile, ".gltf");
        Assert.IsTrue(File.Exists(outputPath), $"Output file not created: {outputPath}");
    }

    #endregion
}
