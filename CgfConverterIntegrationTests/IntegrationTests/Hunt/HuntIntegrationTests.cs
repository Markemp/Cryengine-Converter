using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests.Hunt
{
    [TestClass]
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
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);  // Need to figure out material chunks

            // Visual Scene Check
            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

            // Armature Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("Armature", node.ID);
            Assert.AreEqual("root", node.sID);
            Assert.AreEqual("root", node.Name);
            Assert.AreEqual("JOINT", node.Type.ToString());
            Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);

            // Geometry Node check
            node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
            Assert.AreEqual("assassin_christmas_body.skin", node.ID);
            Assert.AreEqual("assassin_christmas_body.skin", node.Name);
            Assert.AreEqual("NODE", node.Type.ToString());
            Assert.IsNull(node.Instance_Geometry);
            Assert.AreEqual(1, node.Instance_Controller.Length);
            Assert.AreEqual("#Armature", node.Instance_Controller[0].Skeleton[0].Value);

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
            Assert.AreEqual(91, nameArray.Count());
            Assert.IsTrue(nameArray.Contains("L_leg_spiral_01"));

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void AssassinBody()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\Hunt\assassin_bad\assassin_body.skin", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            Assert.AreEqual(0.00000f, cryData.Models[1].RootNode.LocalRotation.m11, TestUtils.delta);

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(0, actualMaterialsCount);   // Need to figure out material chunks

            // Visual Scene Check
            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

            // Armature Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("Armature", node.ID);
            Assert.AreEqual("root", node.sID);
            Assert.AreEqual("root", node.Name);
            Assert.AreEqual("JOINT", node.Type.ToString());
            Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);

            // Geometry Node check
            node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
            Assert.AreEqual("assassin_body.skin", node.ID);
            Assert.AreEqual("assassin_body.skin", node.Name);
            Assert.AreEqual("NODE", node.Type.ToString());
            Assert.IsNull(node.Instance_Geometry);
            Assert.AreEqual(1, node.Instance_Controller.Length);
            Assert.AreEqual("#Armature", node.Instance_Controller[0].Skeleton[0].Value);

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
            Assert.AreEqual(91, nameArray.Count());
            Assert.IsTrue(nameArray.Contains("L_leg_spiral_01"));

            testUtils.ValidateColladaXml(colladaData);
        }
    }
}
