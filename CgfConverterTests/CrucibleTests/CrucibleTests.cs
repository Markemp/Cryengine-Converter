using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CgfConverter;
using CgfConverterTests.TestUtilities;
using CgfConverterTests.Unit_Tests;
using grendgine_collada;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests.CrucibleTests
{
    [TestClass]
    public class CrucibleTests
    {
        private readonly TestUtils testUtils = new TestUtils();

        [TestInitialize]
        public void Initialize()
        {
            testUtils.errors = new List<string>();
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            testUtils.GetSchemaSet();
        }

        [TestMethod]
        public void TechnomancerPillar_ValidateGeometry()
        {
        }
    }
}
