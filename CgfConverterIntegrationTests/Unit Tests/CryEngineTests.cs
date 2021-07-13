using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CgfConverterTests.Unit_Tests
{
    [TestClass]
    public class CryEngineTests
    {
        [TestMethod]
        [ExpectedException(typeof(FileLoadException))]
        public void ProcessCryengineFiles_UnsupportedException()
        {
            var ce = new CryEngine("filename.bad", "datadir");
            ce.ProcessCryengineFiles();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ProcessCryengineFiles_FileNotFoundException()
        {
            var ce = new CryEngine("filename.chr", "datadir");
            ce.ProcessCryengineFiles();
        }
    }
}
