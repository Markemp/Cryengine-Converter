using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Schema;
using System.Xml.Serialization;
using grendgine_collada; // No idea how to actually use this.

namespace CgfConverter
{
    class COLLADA  // class to export to .dae format (COLLADA)
    {
        public XmlSchema schema = new XmlSchema();
        public FileInfo daeOutputFile;
        public XmlDocument daeDoc = new XmlDocument();                      // the COLLADA XML doc
        public CgfData cgfData;                                             // The Cryengine data

        public void WriteCollada(CgfData cryData)  // Write the dae file
        {
            // The root of the functions to write Collada files
            // At this point, we should have a CgfData object, fully populated.
            Console.WriteLine();
            Console.WriteLine("*** Starting WriteCOLLADA() ***");
            Console.WriteLine();

            cgfData = cryData;                                              // cgfData is now a pointer to the data object
            // File name will be "object name.dae"
            daeOutputFile = new FileInfo(cryData.RootNode.Name + ".dae");
            GetSchema();                                                    // Loads the schema.  Needs error checking in case it's offline.
            WriteHeader();
            WriteRootNode();
            WriteAsset();
            daeDoc.Save(daeOutputFile.FullName);
            Console.WriteLine("End of Write Collada");
        }

        public void GetSchema()                                             // Get the schema from kronos.org.  Needs error checking in case it's offline
        {
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.TargetNamespace = "https://www.khronos.org/files/collada_schema_1_5";
        }

        public void WriteHeader()
        {
            //  Write the first line of the collada file
            //  Can't access XmlDeclaration class directly.  Grrr....
            //XmlDeclaration declaration = new XmlDeclaration("1.0","utf-8","",daeDoc);
            XmlDeclaration xmlDecl = daeDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            daeDoc.AppendChild(xmlDecl);
        }

        public void WriteRootNode()
        {
            XmlNode rootNode = daeDoc.CreateElement("COLLADA");
            XmlAttribute rootAttributes = daeDoc.CreateAttribute("xmlns");
            rootAttributes.Value = "http://www.collada.org/2005/11/COLLADASchema";
            rootNode.Attributes.Append(rootAttributes);
            XmlAttribute rootAttributes2 = daeDoc.CreateAttribute("version");
            rootAttributes2.Value = "1.5.0";
            rootNode.Attributes.Append(rootAttributes2);
            daeDoc.AppendChild(rootNode);
        }

        public void WriteAsset()
        {
            // Writes the Asset element in a Collada XML doc
            DateTime fileCreated = DateTime.Now;
            DateTime fileModified = DateTime.Now;           // since this only creates, both times should be the same
            
            Grendgine_Collada_Asset asset = new Grendgine_Collada_Asset();
            Grendgine_Collada_Asset_Contributor contributor = new Grendgine_Collada_Asset_Contributor();
            contributor.Author = "Heffay";
            contributor.Author_Website = "https://github.com/Markemp/Cryengine-Converter";
            contributor.Author_Email = "markemp@gmail.com";
            contributor.Source_Data = cgfData.RootNode.Name;                    // The cgf/cga/skin/whatever file we read
            asset.Created = fileCreated;
            asset.Modified = fileModified;
            asset.Up_Axis = "Z_UP";
            asset.Title = cgfData.RootNode.Name;

            // Add this to xml doc
            XmlNode assetNode = daeDoc.CreateAttribute("asset");
            daeDoc.AppendChild(assetNode);

        }

    }
}
