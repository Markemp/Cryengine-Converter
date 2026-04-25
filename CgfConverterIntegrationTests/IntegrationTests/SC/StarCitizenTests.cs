using CgfConverter;
using CgfConverter.CryEngineCore;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;
using CgfConverter.Renderers.Gltf;
using CgfConverter.Renderers.Gltf.Models;
using CgfConverter.Renderers.USD;
using CgfConverter.Renderers.USD.Models;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class StarCitizenTests
{
    private readonly TestUtils testUtils = new();
    private readonly string objectDir46 = @"d:\depot\sc4.6\data";

    [TestInitialize]
    public void Initialize()
    {
        CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        Thread.CurrentThread.CurrentCulture = customCulture;
        testUtils.GetSchemaSet();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private CryEngine LoadCryData(string relativePath, params string[] extraArgs)
        => LoadCryData(relativePath, includeAnimations: false, extraArgs);

    private CryEngine LoadCryData(string relativePath, bool includeAnimations, params string[] extraArgs)
    {
        var fullPath = $@"{objectDir46}\{relativePath}";
        var args = new[] { fullPath, "-objectdir", objectDir46 }.Concat(extraArgs).ToArray();
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);

        var cryData = new CryEngine(fullPath, testUtils.argsHandler.Args.PackFileSystem,
            new CryEngineOptions(ObjectDir: objectDir46, IncludeAnimations: includeAnimations));
        cryData.ProcessCryengineFiles();
        return cryData;
    }

    private ColladaModelRenderer RenderCollada(string relativePath, params string[] extraArgs)
    {
        var cryData = LoadCryData(relativePath, extraArgs);
        var renderer = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        renderer.GenerateDaeObject();
        return renderer;
    }

    private GltfRoot RenderGltf(string relativePath, params string[] extraArgs)
    {
        var cryData = LoadCryData(relativePath, extraArgs);
        var renderer = new GltfModelRenderer(testUtils.argsHandler.Args, cryData);
        return renderer.GenerateGltfObject();
    }

    private UsdDoc RenderUsd(string relativePath, params string[] extraArgs)
    {
        var cryData = LoadCryData(relativePath, "-usd");
        var renderer = new UsdRenderer(testUtils.argsHandler.Args, cryData);
        return renderer.GenerateUsdObject();
    }

    // ─── Curated tests with strong assertions ───────────────────────────────────
    // These pin behavior on representative assets across .cga / .cgf / .skin / .chr
    // and cover all three renderers (Collada, glTF, USD).

    // ===== default/box.cgf =====

    [TestMethod]
    public void Box_Collada()
    {
        var collada = RenderCollada(@"Objects\default\box.cgf");
        var daeObject = collada.DaeObject;

        var boxNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        var boxMeshNode = boxNode.node;
        Assert.AreEqual("box", boxNode.ID);
        Assert.AreEqual(ColladaNodeType.NODE, boxNode.Type);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", boxNode.Matrix[0].Value_As_String);
        Assert.AreEqual("#grid_grayyellow_mtl_grid_grey-material", boxMeshNode[0].Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);

        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("mesh_box-mesh", geometry.ID);
        Assert.AreEqual(1, daeObject.Library_Geometries.Geometry.Length);
        Assert.AreEqual(4, geometry.Mesh.Source.Length);
        Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(12, geometry.Mesh.Triangles[0].Count);

        var mats = daeObject.Library_Materials;
        Assert.AreEqual(3, mats.Material.Length);
        Assert.AreEqual("grid_grayyellow_mtl_grid_grey", mats.Material[0].Name);

        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void Box_Gltf()
    {
        var gltf = RenderGltf(@"Objects\default\box.cgf");

        Assert.AreEqual(3, gltf.Materials.Count);
        Assert.AreEqual("grid_grey", gltf.Materials[0].Name);
        Assert.AreEqual(4, gltf.Textures.Count);
        Assert.AreEqual(4, gltf.Images.Count);
        Assert.AreEqual(1, gltf.Meshes.Count);
        Assert.AreEqual("mesh_box/mesh", gltf.Meshes[0].Name);
    }

    [TestMethod]
    public void Box_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\default\box.cgf");

        Assert.AreEqual("Z", usdDoc.Header.UpAxis);
        var rootPrim = usdDoc.Prims[0];
        var materialsScope = rootPrim.Children.FirstOrDefault(x => x.Name == "_materials");
        Assert.IsNotNull(materialsScope);
        Assert.IsTrue(materialsScope is UsdScope);
        Assert.IsTrue(materialsScope.Children.Count >= 3);
    }

    // ===== default/teapot.cgf =====

    [TestMethod]
    public void Teapot_Collada()
    {
        var collada = RenderCollada(@"Objects\default\teapot.cgf");
        Assert.IsNotNull(collada.DaeObject.Library_Geometries.Geometry);
        Assert.IsTrue(collada.DaeObject.Library_Geometries.Geometry.Length >= 1);
    }

    [TestMethod]
    public void Teapot_Collada_Unsplit()
    {
        // The -ut flag exercises the texture-unsplit path during rendering
        var collada = RenderCollada(@"Objects\default\teapot.cgf", "-ut");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void Teapot_Gltf()
    {
        var gltf = RenderGltf(@"Objects\default\teapot.cgf");
        Assert.IsTrue(gltf.Meshes.Count >= 1);
        Assert.IsTrue(gltf.Materials.Count >= 1);
    }

    [TestMethod]
    public void Teapot_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\default\teapot.cgf");

        Assert.AreEqual("Z", usdDoc.Header.UpAxis);
        Assert.AreEqual(1, usdDoc.Header.MetersPerUnit);

        var rootPrim = usdDoc.Prims[0];
        Assert.IsTrue(rootPrim is UsdXform);

        var materialsScope = rootPrim.Children.FirstOrDefault(x => x.Name == "_materials");
        Assert.IsNotNull(materialsScope);
        var teapotMaterial = materialsScope.Children.FirstOrDefault(x => x.Name == "teapot_mtl_teapot");
        Assert.IsNotNull(teapotMaterial);
        Assert.IsTrue(teapotMaterial is UsdMaterial);

        var teapotXform = rootPrim.Children.FirstOrDefault(x => x.Name == "teapot");
        Assert.IsNotNull(teapotXform);
        var teapotMesh = teapotXform.Children.FirstOrDefault(x => x is UsdMesh);
        Assert.IsNotNull(teapotMesh);

        var meshAttributes = teapotMesh.Attributes;
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "points"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "faceVertexCounts"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "faceVertexIndices"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "extent"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "normals"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "st"));
        Assert.IsTrue(meshAttributes.Any(a => a.Name == "displayColor"));

        var geomSubsets = teapotMesh.Children.Where(x => x is UsdGeomSubset).ToList();
        Assert.IsTrue(geomSubsets.Count > 0);
        var subset = geomSubsets.FirstOrDefault(x => x.Name == "teapot_mtl_teapot");
        Assert.IsNotNull(subset);
        Assert.IsTrue(subset.Attributes.Any(a => a.Name == "indices"));
        Assert.IsTrue(subset.Attributes.Any(a => a.Name == "material:binding"));
    }

    // ===== AEGS Avenger (.cga) — large scene with hardpoints =====

    [TestMethod]
    public void AEGS_Avenger_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga", "-dds", "-dae");
        var daeObject = collada.DaeObject;

        var noseNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        var leftWing = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[1].node[0];

        Assert.AreEqual("Nose", noseNode.ID);
        var radar = noseNode.node.FirstOrDefault(n => n.ID == "hardpoint_radar");
        Assert.IsNotNull(radar, "Nose should contain a hardpoint_radar child");
        Assert.AreEqual("1 0 0 0 0 1 0 3.925374 0 0 1 -1.074105 0 0 0 1", radar.Matrix[0].Value_As_String);
        Assert.AreEqual("Wing_Left", leftWing.Name);
        Assert.AreEqual("1 0 0 -5.550000 0 1 0 -0.070000 0 0 1 -0.883000 0 0 0 1", leftWing.Matrix[0].Value_As_String);

        Assert.AreEqual(50, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(128, daeObject.Library_Images.Image.Length);

        var noseGeo = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("Nose-mesh", noseGeo.ID);
        Assert.AreEqual(4, noseGeo.Mesh.Source.Length);
        Assert.AreEqual(15, noseGeo.Mesh.Triangles.Length);
        Assert.AreEqual(59817, noseGeo.Mesh.Source[0].Float_Array.Count);
        Assert.IsTrue(noseGeo.Mesh.Source[0].Float_Array.Value_As_String.StartsWith("4.480176 -3.697465 -0.268108"));

        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void AEGS_Avenger_Gltf()
    {
        var gltf = RenderGltf(@"objects\spaceships\ships\aegs\Avenger\AEGS_Avenger.cga");

        Assert.AreEqual(31, gltf.Materials.Count);
        Assert.AreEqual(41, gltf.Meshes.Count);
        Assert.AreEqual(126, gltf.Nodes.Count);
        Assert.IsTrue(gltf.Nodes.Any(n => n.Name == "Front_LG_Door_Right"));
        Assert.IsTrue(gltf.Nodes.Any(n => n.Name == "Front_LG_Door_Left"));
        Assert.IsTrue(gltf.Nodes.Any(n => n.Name == "Canopy"));
        Assert.IsTrue(gltf.Accessors.Count > 200);
    }

    [TestMethod]
    public void AEGS_Avenger_Usd()
    {
        var usdDoc = RenderUsd(@"objects\spaceships\ships\AEGS\Avenger\AEGS_Avenger.cga");

        var rootPrim = usdDoc.Prims[0];
        var materialsScope = rootPrim.Children.FirstOrDefault(x => x.Name == "_materials");
        Assert.IsNotNull(materialsScope);
        Assert.IsTrue(materialsScope.Children.Count > 0);
    }

    // ===== CRUS Spirit Exterior (.cga) — large CGA with hardpoint hierarchy =====

    [TestMethod]
    public void CRUS_Spirit_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\ships\CRUS\spirit\exterior\crus_Spirit.cga");
        var daeObject = collada.DaeObject;

        var bodyNode = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].node[0];
        Assert.AreEqual("body", bodyNode.ID);
        Assert.AreEqual("hardpoint_turret_rear_radar", bodyNode.node[28].ID);
        Assert.AreEqual("1 0 0 0.000001 0 1 0 1 0 0 1 -1.350000 0 0 0 1", bodyNode.node[28].Matrix[0].Value_As_String);
        Assert.AreEqual(134, daeObject.Library_Materials.Material.Length);
        Assert.AreEqual(244, daeObject.Library_Images.Image.Length);

        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void CRUS_Spirit_Gltf()
    {
        var gltf = RenderGltf(@"objects\spaceships\ships\CRUS\spirit\exterior\crus_Spirit.cga");
        Assert.IsTrue(gltf.Materials.Count > 0);
        Assert.IsTrue(gltf.Meshes.Count > 0);
    }

    [TestMethod]
    public void CRUS_Spirit_Usd()
    {
        var usdDoc = RenderUsd(@"objects\spaceships\ships\CRUS\spirit\exterior\crus_Spirit.cga");
        var rootPrim = usdDoc.Prims[0];
        var materialsScope = rootPrim.Children.FirstOrDefault(x => x.Name == "_materials");
        Assert.IsNotNull(materialsScope);
        Assert.IsTrue(materialsScope.Children.Count > 0);
    }

    // ===== MISC Fury (.cga) — deep node hierarchy =====

    [TestMethod]
    public void MISC_Fury_Collada()
    {
        var collada = RenderCollada(@"Objects\spaceships\ships\MISC\Fury\MISC_Fury.cga");
        var visualScene = collada.DaeObject.Library_Visual_Scene;
        var meshWingTopRight = visualScene.Visual_Scene[0].Node[0].node[0].node[72].node[1].node[0].node[0];
        Assert.AreEqual("1 -0 0 -0.848649 0 1 0.000001 -1.239070 -0 -0.000001 1 0.058854 0 0 0 1",
            meshWingTopRight.Matrix[0].Value_As_String);
    }

    [TestMethod]
    public void MISC_Fury_Gltf()
    {
        var gltf = RenderGltf(@"Objects\spaceships\ships\MISC\Fury\MISC_Fury.cga");
        Assert.IsTrue(gltf.Nodes.Count > 0);
    }

    [TestMethod]
    public void MISC_Fury_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\spaceships\ships\MISC\Fury\MISC_Fury.cga");
        Assert.IsNotNull(usdDoc.Prims[0]);
    }

    // ===== med_bay_wall_bed_extender_a (.cgf) — small CGF =====

    [TestMethod]
    public void Med_Bay_Wall_Bed_Extender_A_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Ships\AEGS\Idris_Frigate\interior\med_bay\med_bay_wall_bed_extender_a.cgf");
        var daeObject = collada.DaeObject;

        var visualScene = daeObject.Library_Visual_Scene;
        Assert.AreEqual("med_bay_wall_bed_extender_a", visualScene.Visual_Scene[0].Node[0].Name);
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("med_bay_wall_bed_extender_a-mesh", geometry.ID);
        Assert.AreEqual(4, geometry.Mesh.Triangles.Length);
    }

    [TestMethod]
    public void Med_Bay_Wall_Bed_Extender_A_Gltf()
    {
        var gltf = RenderGltf(@"Objects\Spaceships\Ships\AEGS\Idris_Frigate\interior\med_bay\med_bay_wall_bed_extender_a.cgf");
        Assert.IsTrue(gltf.Meshes.Count >= 1);
    }

    [TestMethod]
    public void Med_Bay_Wall_Bed_Extender_A_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\Spaceships\Ships\AEGS\Idris_Frigate\interior\med_bay\med_bay_wall_bed_extender_a.cgf");
        Assert.IsNotNull(usdDoc.Prims[0]);
    }

    // ===== console_info_banu_1_a (.cgf) — small CGF, also exercises -embedtextures =====

    [TestMethod]
    public void Console_Info_Banu_1_a_Collada()
    {
        var collada = RenderCollada(@"Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Gltf()
    {
        var gltf = RenderGltf(@"Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf");
        Assert.IsNotNull(gltf);
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Gltf_EmbedImages()
    {
        var gltf = RenderGltf(@"Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf", "-embedtextures");
        Assert.AreEqual(@"decal pom-normal/image", gltf.Images[0].Name);
        Assert.IsNull(gltf.Images[0].Uri);
    }

    [TestMethod]
    public void Console_Info_Banu_1_a_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\buildingsets\banu\props\interactive\console\console_info_banu_1_a.cgf");
        Assert.IsNotNull(usdDoc.Prims[0]);
    }

    // ===== f_mobiglas_civilian_01 (.skin) — skinned character =====

    [TestMethod]
    public void Mobiglas_Civilian_01_Collada()
    {
        var collada = RenderCollada(@"Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin");

        var geometries = collada.DaeObject.Library_Geometries.Geometry;
        Assert.AreEqual(1, geometries.Length);

        var materials = collada.DaeObject.Library_Materials.Material;
        Assert.AreEqual(21, materials.Length);

        var visualScene = collada.DaeObject.Library_Visual_Scene.Visual_Scene[0];
        Assert.AreEqual(2, visualScene.Node.Length);
        Assert.AreEqual(ColladaNodeType.JOINT, visualScene.Node[0].Type);
        Assert.AreEqual(ColladaNodeType.NODE, visualScene.Node[1].Type);
        Assert.AreEqual("World", visualScene.Node[0].Name);
        Assert.AreEqual("f_mobiglas_civilian_01", visualScene.Node[1].Name);

        var node0 = visualScene.Node[0];
        var node1 = visualScene.Node[1];
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node1.Matrix[0].Value_As_String);
        Assert.AreEqual(5, node0.node.Length);
        var hipNode = node0.node[2];
        Assert.AreEqual("Hips", hipNode.Name);
        Assert.AreEqual("Hips", hipNode.ID);
        Assert.AreEqual(13, hipNode.node.Length);
        Assert.AreEqual("-0 -0.000002 -1 -0 -0 1 -0.000002 -0.014728 1 0 -0 1.005547 0 0 0 1", hipNode.Matrix[0].Value_As_String);

        var controller = collada.DaeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("#f_mobiglas_civilian_01-mesh", controller.Skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", controller.Skin.Bind_Shape_Matrix.Value_As_String);
    }

    [TestMethod]
    public void Mobiglas_Civilian_01_Gltf()
    {
        var gltf = RenderGltf(@"Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin");
        Assert.IsTrue(gltf.Materials.Count > 0);
        Assert.IsTrue(gltf.Meshes.Count > 0);
    }

    [TestMethod]
    public void Mobiglas_Civilian_01_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\Characters\Mobiglas\f_mobiglas_civilian_01.skin");
        Assert.IsNotNull(usdDoc.Prims[0]);
    }

    // ===== BEHR LaserCannon S2 (.cga) — weapon =====

    [TestMethod]
    public void BEHR_LaserCannon_S2_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga");
        Assert.IsNotNull(collada.DaeObject);
        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void BEHR_LaserCannon_S2_Gltf()
    {
        var gltf = RenderGltf(@"objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga");
        Assert.IsNotNull(gltf);
    }

    [TestMethod]
    public void BEHR_LaserCannon_S2_Usd()
    {
        var usdDoc = RenderUsd(@"objects\spaceships\Weapons\BEHR\BEHR_LaserCannon_S2\BEHR_LaserCannon_S2.cga");
        Assert.IsNotNull(usdDoc.Prims[0]);
    }

    // ===== argo_moth_entrance_lift (.cga) — pins fix/empty-matlayers-nre =====
    // The referenced argo_mole_interior.mtl contains a submaterial with empty
    // <MatLayers/>, which previously crashed parsing and produced a single
    // default material with no submaterials. These tests pin the fix.

    [TestMethod]
    public void Argo_Moth_Entrance_Lift_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Ships\ARGO\moth\exterior\argo_moth_entrance_lift.cga");
        var daeObject = collada.DaeObject;

        var materials = daeObject.Library_Materials.Material;
        // 31 top-level submaterials + flattened MatLayers sub-layers from layered HardSurface materials.
        // Pre-fix this collapsed to a single default material.
        Assert.AreEqual(45, materials.Length, "Material count regression — empty <MatLayers/> handling broken");
        Assert.IsTrue(materials.Any(m => m.Name == "argo_mole_interior_mtl_plastic_2_matte_black"),
            "The empty-MatLayers submaterial must be present");

        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void Argo_Moth_Entrance_Lift_Gltf()
    {
        var gltf = RenderGltf(@"Objects\Spaceships\Ships\ARGO\moth\exterior\argo_moth_entrance_lift.cga");

        Assert.AreEqual(31, gltf.Materials.Count, "Should have all 31 submaterials from argo_mole_interior.mtl");
        Assert.IsTrue(gltf.Materials.Any(m => m.Name == "plastic_2_matte_black"),
            "The empty-MatLayers submaterial must be present");
    }

    [TestMethod]
    public void Argo_Moth_Entrance_Lift_Usd()
    {
        var usdDoc = RenderUsd(@"Objects\Spaceships\Ships\ARGO\moth\exterior\argo_moth_entrance_lift.cga");

        var rootPrim = usdDoc.Prims[0];
        var materialsScope = rootPrim.Children.FirstOrDefault(x => x.Name == "_materials");
        Assert.IsNotNull(materialsScope);
        Assert.AreEqual(31, materialsScope.Children.Count, "Should have all 31 submaterials in _materials scope");

        var emptyLayered = materialsScope.Children.FirstOrDefault(x => x.Name == "argo_mole_interior_mtl_plastic_2_matte_black");
        Assert.IsNotNull(emptyLayered, "The empty-MatLayers submaterial must be present in _materials");
        Assert.IsTrue(emptyLayered is UsdMaterial);

        // At least one mesh should bind to a real material from argo_mole_interior (not a default fallback)
        var meshes = rootPrim.Children
            .Where(c => c.Name != "_materials")
            .SelectMany(FlattenChildren)
            .Where(c => c is UsdMesh)
            .ToList();
        Assert.IsTrue(meshes.Count > 0);
        var anyBinding = meshes
            .SelectMany(m => m.Children)
            .Where(c => c is UsdGeomSubset)
            .Any(s => s.Attributes.Any(a => a.Name == "material:binding"));
        Assert.IsTrue(anyBinding, "At least one GeomSubset should have material:binding");
    }

    private static System.Collections.Generic.IEnumerable<UsdPrim> FlattenChildren(UsdPrim prim)
    {
        yield return prim;
        foreach (var child in prim.Children)
            foreach (var descendant in FlattenChildren(child))
                yield return descendant;
    }

    // ─── Smoke tests ────────────────────────────────────────────────────────────
    // Render-doesn't-throw checks for assets that have historically been
    // problematic. Worth investigating to add stronger assertions over time.

    [TestMethod]
    public void AEGS_Gladius_LandingGear_Front_Chr_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Ships\AEGS\LandingGear\Gladius\AEGS_Gladius_LandingGear_Front_CHR.chr");
        var daeObject = collada.DaeObject;

        Assert.AreEqual("AEGS_Gladius_LandingGear_Front_Anim",
            daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
        Assert.AreEqual(0, daeObject.Library_Geometries.Geometry.Length);
        Assert.AreEqual(55, daeObject.Library_Materials.Material.Length);
    }

    [TestMethod]
    public void AEGS_Idris_Holo_Viewer_Cgf_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\holoviewer_ships\aegs_idris_holo_viewer.cgf");
        var daeObject = collada.DaeObject;

        Assert.AreEqual("AEGS_Idris_holo_viewer", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("AEGS_Idris_holo_viewer-mesh", geometry.ID);
        Assert.AreEqual(4, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(10, daeObject.Library_Materials.Material.Length);
    }

    [TestMethod]
    public void AEGS_Idris_Holo_01_Cga_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\holoviewer_ships\AEGS_Idris_holo_01.cga");
        var daeObject = collada.DaeObject;

        Assert.AreEqual("AEGS_Idris_holo_01", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
    }

    [TestMethod]
    public void AEGS_Vanguard_LandingGear_Front_Skin_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\ships\AEGS\LandingGear\Vanguard\AEGS_Vanguard_LandingGear_Front.skin");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void AEGS_Avenger_LandingGear_Back_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Ships\AEGS\LandingGear\Avenger\AEGS_Avenger_LandingGear_Back.skin");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void ANVL_Arrow_Cga_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\ships\ANVL\Arrow\ANVL_Arrow.cga");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void ANVL_Hurricane_LandingGear_Front_Skin_Collada()
    {
        var cryData = LoadCryData(@"Objects\Spaceships\Ships\ANVL\LandingGear\Hurricane\anvl_hurricane_landing_gear_front_SKIN.skin");
        var mesh = (ChunkMesh)cryData.RootNode.MeshData;
        Assert.AreEqual(-0.443651f, mesh.MinBound.X, TestUtils.delta);
        Assert.AreEqual(-0.2984485f, mesh.MinBound.Y, TestUtils.delta);
        Assert.AreEqual(-2.20503f, mesh.MinBound.Z, TestUtils.delta);
        Assert.AreEqual(0.443650f, mesh.MaxBound.X, TestUtils.delta);
        Assert.AreEqual(3.3411438f, mesh.MaxBound.Y, TestUtils.delta);
        Assert.AreEqual(1.4569355f, mesh.MaxBound.Z, TestUtils.delta);

        var renderer = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        renderer.GenerateDaeObject();
        Assert.IsNotNull(renderer.DaeObject);
    }

    [TestMethod]
    public void ANVL_Valkyrie_Turret_Bubble_Cga_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Turrets\ANVL\Valkyrie\ANVL_Valkyrie_Turret_Bubble.cga");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void Argo_Atls_Powersuit_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\Characters\PowerSuit\ARGO\ATLS\argo_atls_powersuit_l_leg.skin");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void BEHR_Rifle_P4ar_Stock_Cgf_Collada()
    {
        var collada = RenderCollada(@"Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar_stock.cgf");
        Assert.IsNotNull(collada.DaeObject);
        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void BEHR_Rifle_P4ar_Chr_Collada()
    {
        var collada = RenderCollada(@"Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar.chr");
        Assert.AreEqual(44, collada.DaeObject.Library_Materials.Material.Length);
        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void BEHR_Rifle_P4ar_Parts_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\fps_weapons\weapons_v7\behr\rifle\p4ar\brfl_fps_behr_p4ar_parts.skin", "-dds", "-dae");
        Assert.IsNotNull(collada.DaeObject);
        testUtils.ValidateColladaXml(collada);
    }

    [TestMethod]
    public void Bmsl_Apar_Animus_Body_Cga_Collada()
    {
        var collada = RenderCollada(@"Objects\fps_weapons\weapons_v7\apar\launcher\animus\bmsl_fps_apar_animus_body.cga");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void DRAK_Buccaneer_LandingGear_Front_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\Spaceships\Ships\DRAK\Buccaneer\Landing_Gear\DRAK_Buccaneer_Landing_Gear_Front_Skin.skin", "-dds", "-dae");
        Assert.AreEqual(1, collada.DaeObject.Library_Geometries.Geometry.Length);
        Assert.AreEqual(39, collada.DaeObject.Library_Materials.Material.Length);
    }

    [TestMethod]
    public void M_CCC_Vanduul_Helmet_01_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\Characters\Human\male_v7\armor\ccc\m_ccc_vanduul_helmet_01.skin");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void M_CCC_Bear_Helmet_01_Skin_Collada()
    {
        var collada = RenderCollada(@"Objects\Characters\Human\male_v7\armor\ccc\m_ccc_bear_helmet_01.skin");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void M_QRT_Specialist_Heavy_Arms_01_02_Skin_Collada()
    {
        // mtlname chunk doesn't match any material file — exercises dummy-material fallback path
        var collada = RenderCollada(@"Objects\Characters\Human\male_v7\armor\qrt\quirinus\m_qrt_specialist_heavy_arms_01_02.skin", "-dds", "-dae");
        var daeObject = collada.DaeObject;

        Assert.AreEqual("World", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
        var geometry = daeObject.Library_Geometries.Geometry[0];
        Assert.AreEqual("m_qrt_specialist_heavy_arms_01_02-mesh", geometry.ID);
        Assert.AreEqual(2, geometry.Mesh.Triangles.Length);
        Assert.AreEqual(2, daeObject.Library_Materials.Material.Length);
    }

    [TestMethod]
    public void M_NVY_Pilot_Light_Helmet_01_Skin_Collada()
    {
        // Asset uses an explicit material file override
        var fullPath = $@"{objectDir46}\Objects\Characters\Human\male_v7\armor\nvy\pilot_flightsuit\m_nvy_pilot_light_helmet_01.skin";
        var args = new[] { fullPath, "-objectdir", objectDir46 };
        Assert.AreEqual(0, testUtils.argsHandler.ProcessArgs(args));

        CryEngine cryData = new(fullPath, testUtils.argsHandler.Args.PackFileSystem,
            new CryEngineOptions("m_nvy_pilot_light_no_name_01_01_01", objectDir46));
        cryData.ProcessCryengineFiles();

        var renderer = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        renderer.GenerateDaeObject();
        Assert.IsNotNull(renderer.DaeObject);
    }

    [TestMethod]
    public void RSI_Polaris_SeatAccess_Turret_SideLeft_Cga_Collada()
    {
        var collada = RenderCollada(@"objects\spaceships\turrets\rsi\polaris\rsi_polaris_seataccess_turret_sideleft.cga");
        Assert.IsNotNull(collada.DaeObject);
    }

    [TestMethod]
    public void Vgl_Armor_Medium_Helmet_01_Cgf_Collada()
    {
        // Game file references vgl_armor_medium_helmet_01_01_01 but the actual file is m_vgl_armor_medium_helmet_01_01_01.mtl
        var fullPath = $@"{objectDir46}\objects\characters\human\male_v7\armor\vgl\m_vgl_armor_medium_helmet_01.cgf";
        var args = new[] { fullPath, "-objectdir", objectDir46 };
        Assert.AreEqual(0, testUtils.argsHandler.ProcessArgs(args));

        CryEngine cryData = new(fullPath, testUtils.argsHandler.Args.PackFileSystem,
            new CryEngineOptions("m_vgl_armor_medium_helmet_01_01_01", objectDir46));
        cryData.ProcessCryengineFiles();

        var renderer = new ColladaModelRenderer(testUtils.argsHandler.Args, cryData);
        renderer.GenerateDaeObject();
        Assert.IsNotNull(renderer.DaeObject);
    }

    // ─── Animation tests ────────────────────────────────────────────────────────

    [TestMethod]
    public void Aloprat_Skel_Usd_WithCAF()
    {
        var cryData = LoadCryData(@"Objects\Characters\Creatures\aloprat\aloprat_skel.chr", includeAnimations: true, "-usd");

        var usdRenderer = new UsdRenderer(testUtils.argsHandler.Args, cryData);
        var usdDoc = usdRenderer.GenerateUsdObject();
        usdRenderer.Render();

        Assert.AreEqual("Z", usdDoc.Header.UpAxis);
        Assert.IsNotNull(usdDoc.Prims[0]);
        Assert.IsTrue(cryData.CafAnimations.Count > 0,
            "Should have loaded CAF animations from chrparams wildcards");
    }

    [TestMethod]
    public void Aloprat_CAF_IvoAnimation()
    {
        var cryData = LoadCryData(@"Animations\Characters\Creatures\aloprat\ai_aloprat_stand_walk_forward_01.caf",
            includeAnimations: true);

        Assert.IsNotNull(cryData.Models);
        Assert.IsTrue(cryData.Models.Count > 0);

        var model = cryData.Models[0];
        Assert.IsTrue(model.ChunkMap.Values.OfType<ChunkIvoCAF>().Any(),
            "CAF should contain at least one ChunkIvoCAF");
    }
}
