using System;
using System.Xml;
using HoloXPLOR.DataForge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CgfConverterTests
{
    [TestClass]
    public class CryXmlSerializerTests
    {
        [TestMethod]
        public void ReadFile_SCXmlFile()
        {
            string filename = "C:\\Users\\Geoff\\Source\\Repos\\Cryengine Importer\\io_cryengine_importer\\CryXmlB\\asteroid_hangar_landingpad_medium.xmla";

            XmlDocument xml = CryXmlSerializer.ReadFile(filename, true);
        }
    }
}
