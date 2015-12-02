using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Schema;

namespace CgfConverter
{
    class COLLADA  // class to export to .dae format (COLLADA)
    {
        public XmlSchema schema = new XmlSchema();
        public FileInfo daeOutputFile;
        public XmlDocument daeDoc = new XmlDocument();                      // the COLLADA XML doc


        public void GetSchema()                                             // Get the schema from kronos.org.  Needs error checking in case it's offline
        {
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.TargetNamespace = "https://www.khronos.org/files/collada_schema_1_5";
        }

        public void WriteHeader()
        {
            //  Write the first line of the collada file
            XmlDeclaration declaration = new XmlDeclaration();
            declaration.Version = "1.0";
            declaration.Encoding = "utf-8";
            daeDoc.AppendChild(declaration);
        }
        public void WriteCollada(CgfData cgfData)  // Write the dae file
        {
            // The root of the functions to write Collada files
            // At this point, we should have a CgfData object, fully populated.
            Console.WriteLine();
            Console.WriteLine("*** Starting WriteCOLLADA() ***");
            Console.WriteLine();

            // File name will be "object name.blend"
            daeOutputFile = new FileInfo(cgfData.RootNode.Name + ".dae");
            GetSchema();                                                    // Loads the schema.  Needs error checking in case it's offline.
            WriteHeader();

            daeDoc.Save(daeOutputFile.FullName); 
            Console.WriteLine("End of Write Collada");
        }
    }
}
