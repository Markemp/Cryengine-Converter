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

        [TestInitialize]
        public void Initialize()
        {
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            testUtils.GetSchemaSet();
        }

        [TestMethod]
        public void ArcheAge_ChrFileTest()
        {
            var args = new string[] { @"..\..\ResourceFiles\ArcheAge\coupleduckship_foot.chr", "-dds", "-dae" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();
        }
    }
}
