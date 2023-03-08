using CgfConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using CgfConverter.PackFileSystem;

namespace CgfConverterTests.UnitTests
{
    [TestClass]
    public class CryEngineTests
    {
        [TestMethod]
        [ExpectedException(typeof(FileLoadException))]
        public void ProcessCryengineFiles_UnsupportedException()
        {
            var ce = new CryEngine("filename.chr", new RealFileSystem(Path.GetFullPath("datadir")));
            ce.ProcessCryengineFiles();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ProcessCryengineFiles_FileNotFoundException()
        {
            var ce = new CryEngine("filename.chr", new RealFileSystem(Path.GetFullPath("datadir")));
            ce.ProcessCryengineFiles();
        }
    }
}
