using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace CgfConverter
{
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
    }

    public struct Texture               // The texture object.  This is part of a MtlFormat object
    {
        public String Map;              // Diffuse, Specular, Bumpmap, Environment, Heightamp or Custom
        public String File;             // The location of the file.  
    }
    public struct PublicParameters      // After the Textures, general things that apply to the material.  Not really needed
    {
        public uint SpecMapChannelB;
        public uint SpecMapChannelR;
        public Color DirtTint;
        public float DirtGlossFactor;
        public uint GlossMapChannelB;
        // more stuff I'm just not going to implement
    }
    
    public struct MtlFormat 
    {
        // A single material.  Might need to make this IEnumerable.
        public String MaterialName;
        public UInt32 Flags;            // used by Cryengine
        public String Shader;           // Cryengine shader name
        public UInt32 GenMask;          // used by Cryengine, not sure what this is
        public String StringGenMask;    // used by Cryengine, not sure what this is
        public String MatSurfaceType;   // What object the material will be applied to.
        public String MatTemplate;      // Unknown
        public Color Diffuse;           // 3 values for the Diffuse RGB
        public Color Specular;          // 3 values for the Specular RGB
        public Color Emissive;          // 3 values for the Emissive RGB
        public UInt16 Shininess;        // 0-255 value
        public float Opacity;           // 0-1 float
    }
    public class MaterialFile
    {
        // Material information.  This class will read the Cryengine .mtl file, get the material names, and then output a classic .mtl file.
        // If the Cryengine .mtl file doesn't have submaterials, there will just be the one material with the name of the object.
        public StreamWriter b;          // the streamreader of the file
        public CgfData Datafile;        // the cgf datafile.  This will have all the data we need to get the mtl file

        private List<String> MaterialNames = new List<String>();                         // Read the mtl file and get the list of mat names.
        public string[] MaterialNameArray;  // this has the names organized by ID
        private String MtlFileName;         // The name of just the file.  no path, no extension 
        public FileInfo MtlFile;            // the Material file FileInfo
        
        public void GetMtlFileName()        // Get FileInfo Mtlfile name from the MtlName chunks and read it. Assume pwd if no objectdir.
        {
            DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            String[] stringSeparators = new string[] { @"\", @"/" };    // to split up the paths
            String[] result;                                            // carries the results of the split

            Console.WriteLine("*****  In MaterialFile.cs *****");
            Console.WriteLine("Current dir is {0}", currentDir.FullName);
            // Find the number of material chunks.  if 1, then name is the mtl file name.  If many, find type 0x01.
            foreach (CgfData.ChunkMtlName mtlChunk in Datafile.CgfChunks.Where(a => a.chunkType == ChunkType.MtlName))
            {
                mtlChunk.WriteChunk();
                if (mtlChunk.version == 0x800)
                {
                    // this is a parent material. Should be only one.  Don't care about the rest.
                    Console.WriteLine("Type 0x800 found.  {0}", mtlChunk.Name);
                    if (mtlChunk.MatType == 0x01 || mtlChunk.MatType == 0x10)
                    {
                        // parent material.  This is the one we want.
                        Console.WriteLine("Mat type 0x01 found.");
                        if (mtlChunk.Name.Contains(@"\") || mtlChunk.Name.Contains(@"/"))
                        {
                            // I'm a qualified path, but don't know if objectdir exists.  Need to get name+.mtl without the path info.
                            result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                            MtlFileName = result[result.Length - 1];        // Last element in mtlChunk.Name
                            Console.WriteLine("MtlFileName is {0}", MtlFileName);

                            if (Datafile.Args.ObjectDir != null)
                            {
                                MtlFile = new FileInfo(Datafile.Args.ObjectDir + "\\" + mtlChunk.Name + ".mtl");
                                Console.WriteLine("*** MtlFile is {0}", MtlFile.FullName);
                            }
                            else
                            {
                                // No objectdir provided.  Only check current directory.
                                MtlFile = new FileInfo(currentDir + "\\" +  MtlFileName + ".mtl");
                                Console.WriteLine("*** MtlFile is {0}", MtlFile.FullName);
                            }
                            Console.WriteLine("MtlFile.Fullname is {0}", MtlFile.FullName);
                        }
                        else
                        {
                            // mtlchunk.name doesn't have a path.  Just use the current dir + .mtl
                            MtlFile = new FileInfo(currentDir + "\\" + mtlChunk.Name + ".mtl");
                        }
                        if (MtlFile.Exists)
                        {
                            Console.WriteLine("*** Found material file {0}.  Reading it now.", MtlFile.FullName);
                            ReadMtlFile(MtlFile);
                        }
                        else
                        {
                            Console.WriteLine("*** Unable to find material file {0}.  I'm probably going to not work well.", MtlFile.FullName);
                        }
                    }       // Not a material type 0x01 or 0x10.  Will be a child material.  Continue to next mtlname chunk
                }
                else  // version 0x802 file.  There will be just one, so return after it is found and read
                {
                    // Process version 0x802 files
                    Console.WriteLine("In 0x802 section");
                    if (mtlChunk.Name.Contains(@"\") || mtlChunk.Name.Contains(@"/"))
                    {
                        // I'm a qualified path, but don't know if objectdir exists.  Need to get name+.mtl without the path info.
                        result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                        MtlFileName = result[result.Length - 1];        // Last element in mtlChunk.Name
                        Console.WriteLine("MtlFileName is {0}", MtlFileName);

                        if (Datafile.Args.ObjectDir != null)
                        {
                            MtlFile = new FileInfo(Datafile.Args.ObjectDir + "\\" + mtlChunk.Name + ".mtl");
                            Console.WriteLine("*** MtlFile is {0}", MtlFile.FullName);
                        }
                        else
                        {
                            // No objectdir provided.  Only check current directory.
                            MtlFile = new FileInfo(MtlFileName + ".mtl");
                            Console.WriteLine("*** MtlFile is {0}", MtlFileName);
                        }
                        Console.WriteLine("MtlFile.Fullname is {0}", MtlFile.FullName);
                        
                        if (MtlFile.Exists)
                        {
                            // Check object dir + mtlchunk.name + ".mtl"
                            Console.WriteLine("I EXIST!");
                            ReadMtlFile(MtlFile);
                        }
                        else
                        {
                            // Check local directory
                            Console.WriteLine("!!! {0} not found.  Using last part...", MtlFile.FullName);
                            if (mtlChunk.Name.Contains(@"\"))
                            {
                                string s = mtlChunk.Name;
                                string[] parse = s.Split('\\');
                                MtlFile = new FileInfo(currentDir + "\\" + parse[parse.Length - 1] + ".mtl");
                                ReadMtlFile(MtlFile);
                            }
                            if (mtlChunk.Name.Contains(@"/"))
                            {
                                string s = mtlChunk.Name;
                                string[] parse = s.Split('/');
                                MtlFile = new FileInfo(currentDir + "\\" + parse[parse.Length - 1] + ".mtl");
                                ReadMtlFile(MtlFile);
                            }
                        }
                    }
                    else
                    {
                        // It's just a file name.  Search only in current directory.
                        MtlFile = new FileInfo(currentDir + "\\" + mtlChunk.Name + ".mtl");
                        if (MtlFile.Exists)
                        {
                            ReadMtlFile(MtlFile);
                        }
                        else
                        {
                            Console.WriteLine("*** Unable to find a .mtl file.  I will probably crash now. ***");
                        }
                    }
                    return;
                }
            }
            return;
        }
        public void ReadMtlFile(FileInfo Materialfile)    // reads the mtl file, so we can populate the MaterialNames array and assign those material names to the meshes
        {
            // MtlFile should be an object to the Material File for the CgfData.  We need to populate the MaterialNameArray array with the objects.

            Console.WriteLine("Mtl File name is {0}", Materialfile.FullName);
            if (!Materialfile.Exists)
            {
                Console.WriteLine("Unable to find material file {0}.  Using group names.", Materialfile.FullName);
                return;
            }
            XmlTextReader reader = new XmlTextReader(Materialfile.FullName);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        Console.Write("<" + reader.Name);
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
            Console.ReadLine();

            //using (XmlReader reader = XmlReader.Create(Materialfile.FullName))
            //{
            //    while (reader.Read())
            //    {
            //        if (reader.IsStartElement())
            //        {
            //            switch (reader.Name)
            //            {
            //                case "Material":
            //                    {
            //                        // detected this element
            //                        string attribute = reader["Name"];
            //                        //Console.WriteLine(" Name is {0}", attribute);
            //                        if (attribute != null)
            //                        {
            //                            MaterialNames.Add(attribute);
            //                            //Console.WriteLine("  Has attribute Name " + attribute);  // if this works, we want to add to the MaterialNames array
            //                        }
            //                        break;
            //                    }
            //                default:
            //                    {
            //                        break;
            //                    }
            //            }
            //        }
            //    }
            //    if (MaterialNames.Count == 0)
            //    {
            //        Console.WriteLine("No material names found, but there is a material.  Basic mtl file type.  Use object name.");
            //    }
            //}
            int length = MaterialNames.Count;
            Console.WriteLine("{0} Materials found in the material file.", length);
            foreach (string tmpstring in MaterialNames)
            {
                Console.WriteLine("Material name is {0}", tmpstring);
            }
            MaterialNameArray = new String[length];
            MaterialNameArray = MaterialNames.ToArray();
            //Console.WriteLine("Material Name Array length is {0}", MaterialNameArray.Length);
        }
        public void WriteMtlLibFile(StreamWriter file)        // writes the mtllib file to the stream.
        {
            // We don't want to write the mtllib for the Cryengine mtl file.  We want to make a custom old school one for Blender to use.
            // See https://en.wikipedia.org/wiki/Wavefront_.obj_file for specifics.
            string s = string.Format("mtllib {0}", MtlFile.FullName);
            file.WriteLine(s);
        }

    }

}
