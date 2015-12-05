using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
    
    public class MtlFormat 
    {
        // A single material.  Might need to make this IEnumerable.
        public String MaterialName;
        public String Flags;            // used by Cryengine
        public String Shader;           // Cryengine shader name
        public String GenMask;          // used by Cryengine, not sure what this is
        public String StringGenMask;    // used by Cryengine, not sure what this is
        public String SurfaceType;   // What object the material will be applied to.
        public String MatTemplate;      // Unknown
        public Color Diffuse;           // 3 values for the Diffuse RGB
        public Color Specular;          // 3 values for the Specular RGB
        public Color Emissive;          // 3 values for the Emissive RGB
        public float Shininess = 255;        // 0-255 value.  Default to 255 in case there is no value.  This may be wrong.
        public float Opacity = 1;           // 0-1 float. Set to 1 by default in case there is no value
        public List<Texture> Textures = new List<Texture>();      // All the textures for this material
        public PublicParameters PublicParam; // The public parameters, after textures
    }

    public class MaterialFile
    {
        // Material information.  This class will read the Cryengine .mtl file, get the material names, and then output a classic .mtl file.
        // If the Cryengine .mtl file doesn't have submaterials, there will just be the one material with the name of the object.
        public StreamWriter b;          // the streamreader of the file
        public CgfData Datafile;        // the cgf datafile.  This will have all the data we need to get the mtl file

        private List<String> MaterialNames = new List<String>();                        // Read the mtl file and get the list of mat names.
        private List<MtlFormat> Materials = new List<MtlFormat>();                      // List of the material structures
        public MtlFormat[] MaterialNameArray;   // this has the names organized by ID
        private String MtlFileName;             // The name of just the file.  no path, no extension 
        public FileInfo XmlMtlFile;                // the Cryengine xml Material file FileInfo
        public FileInfo MtlFile;                // The obj .mtl file that we write.  Should be RootNode.Name + "_mtl.mtl".
        
        public void GetMtlFileName()        // Get FileInfo Mtlfile name from the MtlName chunks and read it. Assume pwd if no objectdir.
        {
            DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            String[] stringSeparators = new string[] { @"\", @"/" };    // to split up the paths
            String[] result;                                            // carries the results of the split

            //Console.WriteLine("*****  In MaterialFile.cs *****");
            MtlFile = new FileInfo(Datafile.RootNode.Name + "_mtl.mtl");
            // Console.WriteLine("Current dir is {0}", currentDir.FullName);
            // Find the number of material chunks.  if 1, then name is the mtl file name.  If many, find type 0x01.
            foreach (CgfData.ChunkMtlName mtlChunk in Datafile.CgfChunks.Where(a => a.chunkType == ChunkType.MtlName))
            {
                // mtlChunk.WriteChunk();
                if (mtlChunk.version == 0x800)
                {
                    // this is a parent material. Should be only one.  Don't care about the rest.
                    //Console.WriteLine("Type 0x800 found.  {0}", mtlChunk.Name);
                    if (mtlChunk.MatType == 0x01 || mtlChunk.MatType == 0x10)
                    {
                        // parent material.  This is the one we want.
                        //Console.WriteLine("Mat type 0x01 found.");
                        if (mtlChunk.Name.Contains(@"\") || mtlChunk.Name.Contains(@"/"))
                        {
                            // I'm a qualified path, but don't know if objectdir exists.  Need to get name+.mtl without the path info.
                            result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                            MtlFileName = result[result.Length - 1];        // Last element in mtlChunk.Name
                            //Console.WriteLine("MtlFileName (has slash) is {0}", MtlFileName);
                            if (Datafile.Args.ObjectDir != null)                // Check to see if objectdir was set.  if not, just check local dir
                            {
                                XmlMtlFile = new FileInfo(Datafile.Args.ObjectDir + @"\" + mtlChunk.Name + ".mtl");
                            }
                            else
                            {
                                // We were given a objectdir arg, but the material file is local.  Use currentdir.
                                XmlMtlFile = new FileInfo(currentDir + @"\" + MtlFileName + ".mtl");
                            }
                        }
                        else 
                        {
                            MtlFileName = mtlChunk.Name;                    // will add .mtl later.
                            Console.WriteLine("MtlFileName (no slash) is {0}", MtlFileName);
                            XmlMtlFile = new FileInfo(currentDir + @"\" + MtlFileName + ".mtl");
                        }

                        //Console.WriteLine("MtlFile.Fullname is {0}", XmlMtlFile.FullName);

                        if (XmlMtlFile.Exists)
                        {
                            Console.WriteLine("*** Found material file {0}.  Reading it now.", XmlMtlFile.FullName);
                            ReadMtlFile(XmlMtlFile);
                        }
                        else
                        {
                            Console.WriteLine("*** 0x800 Unable to find material file {0}.  I'm probably going to not work well.", XmlMtlFile.FullName);
                        }
                    }       // Not a material type 0x01 or 0x10.  Will be a child material.  Continue to next mtlname chunk
                }
                else  // version 0x802 file.  There will be just one, so return after it is found and read
                {
                    // Process version 0x802 files
                    //Console.WriteLine("In 0x802 section");
                    if (mtlChunk.Name.Contains(@"\") || mtlChunk.Name.Contains(@"/"))
                    {
                        // I'm a qualified path, but don't know if objectdir exists.  Need to get name+.mtl without the path info.
                        result = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                        MtlFileName = result[result.Length - 1];        // Last element in mtlChunk.Name
                        //Console.WriteLine("MtlFileName is {0}", MtlFileName);
                        if (Datafile.Args.ObjectDir != null)
                        {
                            XmlMtlFile = new FileInfo(Datafile.Args.ObjectDir + @"\" + mtlChunk.Name + ".mtl");
                        }
                        else
                        {
                            // No objectdir provided.  Only check current directory.
                            XmlMtlFile = new FileInfo(currentDir + @"\" + MtlFileName + ".mtl");
                        }
                    }
                    else 
                    {
                        // It's just a file name.  Search only in current directory.
                        MtlFileName = mtlChunk.Name;
                        XmlMtlFile = new FileInfo(currentDir + @"\" + MtlFileName + ".mtl");
                        //Console.WriteLine("MtlFileName (short version) is {0}", MtlFileName);
                    }

                    Console.WriteLine("MtlFile.Fullname is {0}", XmlMtlFile.FullName);

                    if (XmlMtlFile.Exists)
                    {
                        Console.WriteLine("*** Found material file {0}.  Reading it now.", XmlMtlFile.FullName);
                        ReadMtlFile(XmlMtlFile);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("*** 0x802 Unable to find material file {0}.  I'm probably going to not work well.", XmlMtlFile.FullName);
                        Console.WriteLine();
                    }
                    return;
                }
            }
            return;
        }
        public void ReadMtlFile(FileInfo Materialfile)          // reads the mtl file, so we can populate the MaterialNames array and assign those material names to the meshes
        {
            // MtlFile should be an object to the Material File for the CgfData.  We need to populate the MaterialNameArray array with the objects.
            // If I was smart I'd actually get Blender to add this to /scripts/addons/io_scene_obj/import_obj.py
            // Console.WriteLine("Mtl File name is {0}", Materialfile.FullName);

            if (!Materialfile.Exists)
            {
                // Set up a dummy material and return.
                Console.WriteLine("Unable to find material file {0}.  Using group names.", Materialfile.FullName);
                MtlFormat material = new MtlFormat();
                material.MaterialName = Datafile.RootNode.Name;                 // since there is no Name, use the node name.
                material.Diffuse.Red = 1;
                material.Diffuse.Green = 0;
                material.Diffuse.Blue = 1;
                Materials.Add(material);
                MaterialNameArray = new MtlFormat[Materials.Count];
                MaterialNameArray = Materials.ToArray();
                return;
            }
            XElement materialmap = XElement.Load(Materialfile.FullName);

            if (materialmap.Attribute("MtlFlags").Value == "524288")
            {
                // Short mtl file with just one material.  No name, so set it to the object name.
                //Console.WriteLine("Found material with 524288");
                MtlFormat material = new MtlFormat();
                //Console.WriteLine("Attribute: {0}", Datafile.RootNode.Name);
                material.MaterialName = Datafile.RootNode.Name;                 // since there is no Name, use the node name.
                material.Flags = materialmap.Attribute("MtlFlags").Value;
                material.Shader = materialmap.Attribute("Shader").Value;
                material.GenMask = materialmap.Attribute("GenMask").Value;
                material.StringGenMask = materialmap.Attribute("StringGenMask").Value;
                material.SurfaceType = materialmap.Attribute("SurfaceType").Value;
                material.MatTemplate = materialmap.Attribute("MatTemplate").Value;
                String tmpDiffuse = materialmap.Attribute("Diffuse").Value;
                string[] parse = tmpDiffuse.Split(',');
                material.Diffuse.Red = float.Parse(parse[0]);
                material.Diffuse.Green = float.Parse(parse[1]);
                material.Diffuse.Blue = float.Parse(parse[2]);
                String tmpSpec = materialmap.Attribute("Specular").Value;
                string[] parsespec = tmpSpec.Split(',');
                material.Specular.Red = float.Parse(parsespec[0]);
                material.Specular.Blue = float.Parse(parsespec[1]);
                material.Specular.Green = float.Parse(parsespec[2]);
                String tmpEmissive = materialmap.Attribute("Emissive").Value;
                string[] parseemissive = tmpEmissive.Split(',');
                material.Emissive.Red = float.Parse(parseemissive[0]);
                material.Emissive.Blue = float.Parse(parseemissive[1]);
                material.Emissive.Green = float.Parse(parseemissive[2]);
                material.Shininess = float.Parse(materialmap.Attribute("Shininess").Value);
                material.Opacity = float.Parse(materialmap.Attribute("Opacity").Value);
                // now loop for all the textures
                int i = 0;
                foreach (XElement tex in materialmap.Descendants("Texture"))
                {
                    Texture temptex = new Texture();
                    temptex.Map = tex.Attribute("Map").Value;
                    temptex.File = tex.Attribute("File").Value;
                    material.Textures.Add(temptex);
                    //Console.WriteLine("Texture File found: {0}", material.Textures[i].File);
                    i++;
                }
                Materials.Add(material);
            }
            else            // more complicated material file, with Submaterials attribute
            {
                foreach (XElement mat in materialmap.Descendants("SubMaterials"))
                {
                    //var matElement = mat.Element("Material");
                    //Console.WriteLine("SubMat {0}", mat);
                    foreach (XElement submat in mat.Descendants("Material"))
                    {
                        MtlFormat material = new MtlFormat();
                        //Console.WriteLine("Attribute: {0}", submat.Attribute("Name"));
                        material.MaterialName = submat.Attribute("Name").Value;
                        material.Flags = submat.Attribute("MtlFlags").Value;
                        material.Shader = submat.Attribute("Shader").Value;
                        material.GenMask = submat.Attribute("GenMask").Value;
                        material.StringGenMask = submat.Attribute("StringGenMask").Value;
                        material.SurfaceType = submat.Attribute("SurfaceType").Value;
                        material.MatTemplate = submat.Attribute("MatTemplate").Value;
                        String tmpDiffuse = submat.Attribute("Diffuse").Value;
                        string[] parse = tmpDiffuse.Split(',');
                        material.Diffuse.Red = float.Parse(parse[0]);
                        material.Diffuse.Green = float.Parse(parse[1]);
                        material.Diffuse.Blue = float.Parse(parse[2]);
                        String tmpSpec = submat.Attribute("Specular").Value;
                        string[] parsespec = tmpSpec.Split(',');
                        material.Specular.Red = float.Parse(parsespec[0]);
                        material.Specular.Blue = float.Parse(parsespec[1]);
                        material.Specular.Green = float.Parse(parsespec[2]);
                        String tmpEmissive = submat.Attribute("Emissive").Value;
                        string[] parseemissive = tmpEmissive.Split(',');
                        material.Emissive.Red = float.Parse(parseemissive[0]);
                        material.Emissive.Blue = float.Parse(parseemissive[1]);
                        material.Emissive.Green = float.Parse(parseemissive[2]);
                        if (submat.Attributes("Shininess").ToString() != "")
                        {
                            material.Shininess = float.Parse(submat.Attribute("Shininess").Value);  
                        }
                        //Console.WriteLine("Submat: {0}", submat.Attribute("Opacity"));
                        if (submat.Attribute("Opacity") != null )             // Default is set to 1, but if it exists grab the value.
                        { 
                            material.Opacity = float.Parse(submat.Attribute("Opacity").Value);
                        }
                        // now loop for all the textures
                        int i = 0;
                        foreach (XElement tex in submat.Descendants("Texture"))
                        {
                            Texture temptex = new Texture();
                            temptex.Map = tex.Attribute("Map").Value;
                            temptex.File = tex.Attribute("File").Value;
                            material.Textures.Add(temptex);
                            //Console.WriteLine("Texture File found: {0}", material.Textures[i].File);
                            i++;
                        }
                        Materials.Add(material);
                    }
                }
            }

            int length = Materials.Count;
            //Console.WriteLine("{0} Materials found in the material file.", length);
            //foreach (MtlFormat mats in Materials)
            //{
            //    Console.WriteLine("Material name is {0}", mats.MaterialName);
            //}
            MaterialNameArray = new MtlFormat[Materials.Count];
            MaterialNameArray = Materials.ToArray();
        }
        public void WriteMtlLibInfo(StreamWriter file)          // writes the mtllib file to the stream.
        {
            // We don't want to write the mtllib for the Cryengine mtl file.  We want to make a custom old school one for Blender to use.
            // See https://en.wikipedia.org/wiki/Wavefront_.obj_file for specifics.
            string s = string.Format("mtllib {0}", MtlFile.Name);
            file.WriteLine(s);
            WriteMtlFile();                                     // write the _mtl.mtl file
        }
        public void WriteMtlFile()                              // writes the .mtl file for the .obj file we create.
        {
            // Write the .mtl file for the .obj file we create.  This will be rootnode name + _mtl.mtl.
            FileInfo mtlFile = new FileInfo(MtlFile.Name);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(mtlFile.Name))
            {
                string s = String.Format("# Material file output from cgf-converter.exe version 0.8");
                file.WriteLine(s);
                file.WriteLine("#");
                foreach (MtlFormat mtl in Materials)
                {
                    // start writing each material.
                    // See https://en.wikipedia.org/wiki/Wavefront_.obj_file for format structure
                    string s_name = String.Format("newmtl {0}", mtl.MaterialName);
                    file.WriteLine(s_name);
                    // write Kd, Ks, d
                    string s_diffuse = String.Format("Kd {0:F4} {1:F4} {2:F4}", mtl.Diffuse.Red, mtl.Diffuse.Green, mtl.Diffuse.Blue);
                    file.WriteLine(s_diffuse);
                    string s_spec = String.Format("Ks  {0:F4} {1:F4} {2:F4}", mtl.Specular.Red, mtl.Specular.Green, mtl.Specular.Blue);
                    file.WriteLine(s_spec);
                    string s_dissolve = String.Format("d {0:F4}", mtl.Opacity);
                    file.WriteLine(s_dissolve);
                    file.WriteLine("illum 2");  // Highlight on.  this is a guess.
                    int length = mtl.Textures.Count;   // number of texture files.
                    for (int i = 0; i < length; i++)
                    {
                        // replace .tif filenames with .dds
                        // If there is no path (\ or /) in the file, then don't put in Object Dir!!
                        StringBuilder builder;
                        if (mtl.Textures[i].File.Contains(@"/") || mtl.Textures[i].File.Contains(@"\"))
                        {
                            builder = new StringBuilder(Datafile.Args.ObjectDir + @"\" + mtl.Textures[i].File);
                        }
                        else
                        {
                            builder = new StringBuilder(mtl.Textures[i].File);
                        }

                        builder.Replace(".tif", ".dds");
                        switch (mtl.Textures[i].Map)
                        {
                            case "Diffuse":
                                {
                                    string s_mapdiffuse = String.Format("map_Kd {0}", builder.ToString());
                                    file.WriteLine(s_mapdiffuse);
                                    break;
                                }
                            case "Specular":
                                {
                                    string s_mapspec = String.Format("map_Ks {0}", builder.ToString());
                                    file.WriteLine(s_mapspec);
                                    break;
                                }
                            case "Bumpmap":
                                {
                                    string s_mapbump = String.Format("map_bump {0}", builder.ToString());
                                    file.WriteLine(s_mapbump);
                                    break;
                                }
                            case "Detail":
                                {
                                    string s_mapbump = String.Format("map_bump {0}", builder.ToString());
                                    file.WriteLine(s_mapbump);
                                    break;
                                }
                            //case "Environment":                 // For things like cockpit monitors.
                            //    {
                            //        string s_env = String.Format("map_Kd {0}", builder.ToString());
                            //        file.WriteLine(s_env);
                            //        break;
                            //    }
                            default:
                                break;
                        }
                    }
                    file.WriteLine();
                }

            }
        }               // end WriteMtlFile method
    }                   // end MaterialFile class
}
