using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CgfConverter.Renderers.Collada;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverterTests.IntegrationTests;

[TestClass]
[TestCategory("integration")]
public class HuntIntegrationTests
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
    public void AssassinChristmasBody()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Hunt\assassin_good\assassin_christmas_body.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(5, actualMaterialsCount);  // Need to figure out material chunks

        // Visual Scene Check
        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature Node check
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("root", node.ID);
        Assert.AreEqual("root", node.sID);
        Assert.AreEqual("root", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("1 0 0 -0 0 1 0 -0 0 0 1 -0 0 0 0 1", node.Matrix[0].Value_As_String);

        // Geometry Node check
        node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
        Assert.AreEqual("assassin_christmas_body", node.ID);
        Assert.AreEqual("assassin_christmas_body", node.Name);
        Assert.AreEqual("NODE", node.Type.ToString());
        Assert.IsNull(node.Instance_Geometry);
        Assert.AreEqual(1, node.Instance_Controller.Length);
        Assert.AreEqual("#root", node.Instance_Controller[0].Skeleton[0].Value);

        // Controller check
        var controller = daeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        var skin = controller.Skin;
        Assert.AreEqual("#assassin_christmas_body-mesh", skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
        Assert.AreEqual(3, skin.Source.Length);
        var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
        var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
        var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
        var joints = skin.Joints;

        Assert.AreEqual(91, controllerJoints.Name_Array.Count);
        Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);
        var nameArray = controllerJoints.Name_Array.Value();
        Assert.AreEqual(91, nameArray.Length);
        Assert.IsTrue(nameArray.Contains("L_leg_spiral_01"));

        testUtils.ValidateColladaXml(colladaData);
    }

    [TestMethod]
    public void AssassinBody()
    {
        var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Hunt\assassin_bad\assassin_body.skin", "-dds", "-dae" };
        int result = testUtils.argsHandler.ProcessArgs(args);
        Assert.AreEqual(0, result);
        CryEngine cryData = new(args[0], testUtils.argsHandler.PackFileSystem);
        cryData.ProcessCryengineFiles();

        Assert.AreEqual(1.00000f, cryData.Models[1].RootNode.LocalTransform.M11, TestUtils.delta);

        ColladaModelRenderer colladaData = new(testUtils.argsHandler, cryData);
        var daeObject = colladaData.DaeObject;
        colladaData.GenerateDaeObject();

        int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
        Assert.AreEqual(5, actualMaterialsCount);   // Need to figure out material chunks

        // Visual Scene Check
        Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
        Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
        Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
        Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
        Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

        // Armature Node check
        var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
        Assert.AreEqual("root", node.ID);
        Assert.AreEqual("root", node.sID);
        Assert.AreEqual("root", node.Name);
        Assert.AreEqual("JOINT", node.Type.ToString());
        Assert.AreEqual("1 0 0 -0 0 1 0 -0 0 0 1 -0 0 0 0 1", node.Matrix[0].Value_As_String);

        // Geometry Node check
        node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
        Assert.AreEqual("assassin_body", node.ID);
        Assert.AreEqual("assassin_body", node.Name);
        Assert.AreEqual("NODE", node.Type.ToString());
        Assert.IsNull(node.Instance_Geometry);
        Assert.AreEqual(1, node.Instance_Controller.Length);
        Assert.AreEqual("#root", node.Instance_Controller[0].Skeleton[0].Value);

        // Controller check
        var controller = daeObject.Library_Controllers.Controller[0];
        Assert.AreEqual("Controller", controller.ID);
        var skin = controller.Skin;
        Assert.AreEqual("#assassin_body-mesh", skin.source);
        Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
        Assert.AreEqual(3, skin.Source.Length);
        var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
        var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
        var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
        var joints = skin.Joints;

        Assert.AreEqual(91, controllerJoints.Name_Array.Count);
        Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);
        var nameArray = controllerJoints.Name_Array.Value();
        Assert.AreEqual(91, nameArray.Length);
        Assert.IsTrue(nameArray.Contains("L_leg_spiral_01"));

        // Geometry Library check
        var geometry = daeObject.Library_Geometries.Geometry;
        Assert.AreEqual("assassin_body-mesh", geometry[0].ID);
        Assert.AreEqual("assassin_body", geometry[0].Name);
        var mesh = geometry[0].Mesh;
        var sources = mesh.Source;
        Assert.AreEqual(4, sources.Length);
        Assert.IsNotNull(mesh.Vertices);
        Assert.IsNull(mesh.Trifans);
        var triangles = mesh.Triangles[0];
        Assert.AreEqual(10213, triangles.Count);
        Assert.AreEqual(4, triangles.Input.Length);
        Assert.AreEqual(ColladaInputSemantic.VERTEX, triangles.Input[0].Semantic);
        Assert.AreEqual("#assassin_body-vertices", triangles.Input[0].source);
        Assert.AreEqual(0, triangles.Input[0].Offset);
        Assert.AreEqual(0, triangles.Input[0].Set);
        Assert.AreEqual(ColladaInputSemantic.NORMAL, triangles.Input[1].Semantic);
        Assert.AreEqual("#assassin_body-mesh-norm", triangles.Input[1].source);
        Assert.AreEqual(1, triangles.Input[1].Offset);
        Assert.AreEqual(0, triangles.Input[1].Set);
        Assert.AreEqual(ColladaInputSemantic.TEXCOORD, triangles.Input[2].Semantic);
        Assert.AreEqual("#assassin_body-mesh-UV", triangles.Input[2].source);
        Assert.AreEqual(2, triangles.Input[2].Offset);
        Assert.IsTrue(triangles.P.Value_As_String.StartsWith("0 0 0 0 1 1 1 1 2 2 2 2 2 2 2 2 1 1 1 1 3 3 3 3 4 4 4 4 1 1 1 1 0 0 0 0 2 2 2 2 3 3 3 3 5 5 5 5 6 6 6 6 1"));
        Assert.AreEqual(ColladaInputSemantic.COLOR, triangles.Input[3].Semantic);
        Assert.AreEqual("#assassin_body-mesh-color", triangles.Input[3].source);
        Assert.AreEqual(3, triangles.Input[3].Offset);

        // Geometry Source checks
        var vertices = mesh.Source[0];
        var normals = mesh.Source[1];
        var uvs = mesh.Source[2];
        var colors = mesh.Source[3];
        Assert.AreEqual("assassin_body-mesh-pos", vertices.ID);
        Assert.AreEqual("assassin_body-pos", vertices.Name);
        Assert.AreEqual("assassin_body-mesh-norm", normals.ID);
        Assert.AreEqual("assassin_body-norm", normals.Name);
        Assert.AreEqual("assassin_body-mesh-UV", uvs.ID);
        Assert.AreEqual("assassin_body-UV", uvs.Name);
        Assert.AreEqual("assassin_body-mesh-color", colors.ID);
        Assert.AreEqual("assassin_body-color", colors.Name);
        Assert.AreEqual(18087, vertices.Float_Array.Count);
        Assert.AreEqual("assassin_body-mesh-pos-array", vertices.Float_Array.ID);
        Assert.IsTrue(vertices.Float_Array.Value_As_String.StartsWith("0.050568 0.100037 2.091797 0.048096 0.124878 2.099609 0.059082 0.102295 2.117188 0.049042 0.124695 2.113281 0.016800 0.109009"));
        Assert.IsTrue(colors.Float_Array.Value_As_String.StartsWith("0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0"));
        Assert.AreEqual((uint)6029, vertices.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, vertices.Technique_Common.Accessor.Stride);
        Assert.AreEqual(18087, normals.Float_Array.Count);
        Assert.AreEqual((uint)6029, normals.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)3, normals.Technique_Common.Accessor.Stride);
        Assert.AreEqual(12058, uvs.Float_Array.Count);
        Assert.AreEqual((uint)6029, uvs.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)2, uvs.Technique_Common.Accessor.Stride);
        Assert.AreEqual(24116, colors.Float_Array.Count);
        Assert.AreEqual((uint)6029, colors.Technique_Common.Accessor.Count);
        Assert.AreEqual((uint)4, colors.Technique_Common.Accessor.Stride);

        testUtils.ValidateColladaXml(colladaData);
    }
}
