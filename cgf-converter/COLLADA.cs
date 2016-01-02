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
        public XmlDocument daeDoc = new XmlDocument();                      // the COLLADA XML doc.  Going to try serialized instead.
        public CgfData cgfData;                                             // The Cryengine data
        public Grendgine_Collada daeObject = new Grendgine_Collada();       // This is the serializable class.
        XmlSerializer mySerializer = new XmlSerializer(typeof(Grendgine_Collada));

        //[XmlRootAttribute("COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema",IsNullable = false)]

        public void WriteCollada(CgfData dataFile)  // Write the dae file
        {
            // The root of the functions to write Collada files
            // At this point, we should have a CgfData object, fully populated.
            Console.WriteLine();
            Console.WriteLine("*** Starting WriteCOLLADA() ***");
            Console.WriteLine();

            cgfData = dataFile;                                              // cgfData is now a pointer to the data object
            // File name will be "object name.dae"
            daeOutputFile = new FileInfo(cgfData.RootNode.Name + ".dae");
            GetSchema();                                                    // Loads the schema.  Needs error checking in case it's offline.
            //CreateRootNode();
            CreateAsset();
            WriteLibrary_Images();
            //daeDoc.Save(daeOutputFile.FullName);
            TextWriter writer = new StreamWriter(daeOutputFile.FullName);   // Makes the Textwriter object for the output
            mySerializer.Serialize(writer, daeObject);                      // Serializes the daeObject and writes to the writer
            Console.WriteLine("End of Write Collada");
        }

        public void GetSchema()                                             // Get the schema from kronos.org.  Needs error checking in case it's offline
        {
            schema.ElementFormDefault = XmlSchemaForm.Qualified;
            schema.TargetNamespace = "https://www.khronos.org/files/collada_schema_1_5";
        }


        public void CreateAsset()
        {
            // Writes the Asset element in a Collada XML doc
            DateTime fileCreated = DateTime.Now;
            DateTime fileModified = DateTime.Now;           // since this only creates, both times should be the same
            Grendgine_Collada_Asset asset = new Grendgine_Collada_Asset();

            Grendgine_Collada_Asset_Contributor [] contributors = new Grendgine_Collada_Asset_Contributor[2];
            contributors[0] = new Grendgine_Collada_Asset_Contributor();
            contributors[0].Author = "Heffay";
            contributors[0].Author_Website = "https://github.com/Markemp/Cryengine-Converter";
            contributors[0].Author_Email = "markemp@gmail.com";
            contributors[0].Source_Data = cgfData.RootNode.Name;                    // The cgf/cga/skin/whatever file we read
            // Get the actual file creators from the Source Chunk
            contributors[1] = new Grendgine_Collada_Asset_Contributor();
            foreach (CgfData.ChunkSourceInfo tmpSource in cgfData.CgfChunks.Where(a => a.chunkType == ChunkType.SourceInfo))
            {
                contributors[1].Author = tmpSource.Author;
                contributors[1].Source_Data = tmpSource.SourceFile;
            }
            asset.Created = fileCreated;
            asset.Modified = fileModified;
            asset.Up_Axis = "Z_UP";
            asset.Title = cgfData.RootNode.Name;
            daeObject.Asset = asset;
            daeObject.Asset.Contributor = contributors;

        }
        public void WriteLibrary_Images()
        {
            // I think this is a  list of all the images used by the asset.
            // Following the Noesis lead, we're going to make a dummy record here, that points to objectdir
            Grendgine_Collada_Library_Images libraryImages = new Grendgine_Collada_Library_Images();
            
            Grendgine_Collada_Image [] image1 = new Grendgine_Collada_Image[1];
            image1[0] = new Grendgine_Collada_Image();
            image1[0].ID = "image";
            Console.WriteLine("Image is {0}", cgfData.Args.ObjectDir.ToString());
            image1[0].Init_From = new Grendgine_Collada_Init_From();
            image1[0].Init_From.Ref = cgfData.Args.ObjectDir.ToString();
            daeObject.Library_Images = libraryImages;
            daeObject.Library_Images.Image = image1;
        }
        public void WriteLibrary_Materials()
        {

        }
        public void WriteLibrary_Effects()
        {

        }
        public void WriteLibrary_Geometries()
        {

        }
        public void WriteLibrary_Controllers()
        {

        }
        public void WriteLibrary_VisualScenes()
        {

        }
        public void WriteScene()
        {

        }

    }
}
