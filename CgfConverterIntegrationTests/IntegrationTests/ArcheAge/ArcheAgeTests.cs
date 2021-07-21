using CgfConverter;
using CgfConverterTests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace CgfConverterTests.IntegrationTests.ArcheAge
{
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
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Length;
            Assert.AreEqual(6, actualMaterialsCount);

            // Visual Scene Check 
            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

            // Armature Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("Armature", node.ID);
            Assert.AreEqual("Bip01", node.sID);
            Assert.AreEqual("Bip01", node.Name);
            Assert.AreEqual("JOINT", node.Type.ToString());
            Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);
            var pelvisNode = node.node[0];
            Assert.AreEqual("Armature_Bip01_Pelvis", pelvisNode.ID);
            Assert.AreEqual("Bip01_Pelvis", pelvisNode.Name);
            Assert.AreEqual("Bip01_Pelvis", pelvisNode.sID);
            Assert.AreEqual("JOINT", pelvisNode.Type.ToString());
            Assert.AreEqual("0 1 0 0 -0 -0 1 -0.000001 1 -0 0 8.346858 0 0 0 1", pelvisNode.Matrix[0].Value_As_String);
            Assert.AreEqual(3, pelvisNode.node.Length);

        }
    }
}
