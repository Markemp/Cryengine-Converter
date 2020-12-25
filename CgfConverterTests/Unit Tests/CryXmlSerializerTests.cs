using HoloXPLOR.DataForge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;

namespace CgfConverterTests.IntegrationTests
{
    [TestClass]
    public class CryXmlSerializerTests
    {
        string userHome;

        [TestInitialize]
        public void Initialize()
        {
            userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }


        [TestMethod]
        public void ReadFile_SCXmlFile()
        {
            string filename = $@"{userHome}\OneDrive\ResourceFiles\SC\asteroid_hangar_landingpad_medium.xmla";

            XmlDocument xml = CryXmlSerializer.ReadFile(filename, true);
            //TODO:  Complete this
        }
    }
}
