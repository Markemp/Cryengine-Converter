using HoloXPLOR.DataForge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

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
            //TODO:  Complete this
        }
    }
}
