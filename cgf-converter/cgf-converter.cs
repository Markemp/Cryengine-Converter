using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Math;
using System.Xml;
using System.Xml.Schema;

namespace CgfConverter
{
    public class CgfData // Stores all information about the cgf file format.
    {
        // Header, ChunkTable and Chunks are what are in a file.  1 header, 1 table, and a chunk for each entry in the table.
        public FileInfo DataFile;
        public FileInfo objOutputFile;          // want this to be the datafile minus the extension and .obj
        private static int FileVersion;
        private static uint NumChunks;          // number of chunks in the chunk table

        public Header CgfHeader;
        public ChunkTable CgfChunkTable = new ChunkTable();                    // CgfChunkTable contains a list of all the Chunks.
        public List<Chunk> CgfChunks = new List<Chunk>();   //  I don't think we want this.  Dictionary is better because of ID
        public Dictionary<UInt32, Chunk> ChunkDictionary = new Dictionary<UInt32, Chunk>();  // ChunkDictionary will let us get the Chunk from the ID.
        private UInt32 RootNodeID = 0x0;
        public ChunkNode RootNode;
        public List<ChunkHeader> ChunkHeaders = new List<ChunkHeader>();

        public UInt32 CurrentVertexPosition = 0; //used to recalculate the face indices for files with multiple objects (o)
        public UInt32 TempVertexPosition = 0;
        public UInt32 CurrentIndicesPosition = 0;
        public UInt32 TempIndicesPosition = 0;

        const String BasePath = @"E:\Blender Projects\Mechs\";  // for testing.  This will eventually need to be input by user.
        public ArgsHandler Args = new ArgsHandler();

        public MaterialFile MatFile = new MaterialFile();    // The material file (from MaterialFile.cs)

        public void GetCgfData(FileInfo inputFile)          // Does the actual reading.  Called from ReadCgfData, which sets up the data structure.
        {
            ChunkTable tmpChunkTable = new ChunkTable();    // Will add this to the cgfdata structure after it's read
            List<ChunkHeader> tmpChunkHeaders = new List<ChunkHeader>();
            // Open the file for reading.
            BinaryReader cgfReader = new BinaryReader(File.Open(inputFile.FullName.ToString(), FileMode.Open));
            // Get the header.  This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
            CgfHeader = new Header();                       // Gets the header of the file (3-5 objects dep on version)
            CgfHeader.GetHeader(cgfReader);
            cgfReader.BaseStream.Seek(CgfHeader.fileOffset, 0);  // will now start to read from the start of the chunk table
            tmpChunkTable.GetChunkTable(cgfReader, CgfHeader.fileOffset);
            //tmpChunkTable.WriteChunk();
            // Add this temp chunk table to the main table.  That will contain the full list of chunks added to the dictionary
            CgfChunkTable.chkHeaders.AddRange(tmpChunkTable.chkHeaders);

            foreach (ChunkHeader ChkHdr in tmpChunkTable.chkHeaders)
            {
                //Console.WriteLine("Processing {0}", ChkHdr.type);
                switch (ChkHdr.type)
                {
                    case ChunkType.SourceInfo:
                        {
                            ChunkSourceInfo chkSrcInfo = new ChunkSourceInfo();
                            chkSrcInfo.version = ChkHdr.version;
                            chkSrcInfo.id = ChkHdr.id;
                            chkSrcInfo.size = ChkHdr.size;
                            chkSrcInfo.ReadChunk(cgfReader, ChkHdr.offset);
                            CgfChunks.Add(chkSrcInfo);
                            ChunkDictionary.Add(chkSrcInfo.id, chkSrcInfo);
                            //chkSrcInfo.WriteChunk(); 
                            break;
                        }
                    case ChunkType.Timing:
                        {
                            // Timing chunks don't have IDs for some reason.
                            ChunkTimingFormat chkTiming = new ChunkTimingFormat();
                            chkTiming.ReadChunk(cgfReader, ChkHdr.offset);
                            chkTiming.id = ChkHdr.id;
                            CgfChunks.Add(chkTiming);
                            ChunkDictionary.Add(chkTiming.id, chkTiming);
                            //chkTiming.WriteChunk();
                            break;
                        }
                    case ChunkType.ExportFlags:
                        {
                            ChunkExportFlags chkExportFlag = new ChunkExportFlags();
                            chkExportFlag.ReadChunk(cgfReader, ChkHdr.offset);
                            chkExportFlag.id = ChkHdr.id;
                            chkExportFlag.chunkType = ChkHdr.type;
                            CgfChunks.Add(chkExportFlag);
                            ChunkDictionary.Add(chkExportFlag.id, chkExportFlag);
                            //chkExportFlag.WriteChunk();
                            break;
                        }
                    case ChunkType.Mtl:
                        {
                            //Console.WriteLine("Mtl Chunk here");  // Obsolete.  Not used?
                            break;
                        }
                    case ChunkType.MtlName:
                        {
                            ChunkMtlName chkMtlName = new ChunkMtlName();
                            chkMtlName.version = ChkHdr.version;
                            chkMtlName.id = ChkHdr.id;  // Should probably check to see if the 2 values match...
                            chkMtlName.chunkType = ChkHdr.type;
                            chkMtlName.size = ChkHdr.size;
                            chkMtlName.ReadChunk(cgfReader, ChkHdr.offset);
                            CgfChunks.Add(chkMtlName);
                            ChunkDictionary.Add(chkMtlName.id, chkMtlName);
                            //chkMtlName.WriteChunk();
                            break;
                        }
                    case ChunkType.DataStream:
                        {
                            ChunkDataStream chkDataStream = new ChunkDataStream();
                            chkDataStream.id = ChkHdr.id;
                            chkDataStream.chunkType = ChkHdr.type;
                            chkDataStream.version = ChkHdr.version;
                            chkDataStream.ReadChunk(cgfReader, ChkHdr.offset);
                            CgfChunks.Add(chkDataStream);
                            ChunkDictionary.Add(chkDataStream.id, chkDataStream);
                            //chkDataStream.WriteChunk();
                            break;
                        }

                    case ChunkType.Mesh:
                        {
                            ChunkMesh chkMesh = new ChunkMesh();
                            chkMesh.id = ChkHdr.id;
                            chkMesh.chunkType = ChkHdr.type;
                            chkMesh.version = ChkHdr.version;
                            chkMesh.ReadChunk(cgfReader, ChkHdr.offset);
                            CgfChunks.Add(chkMesh);
                            ChunkDictionary.Add(chkMesh.id, chkMesh);
                            //chkMesh.WriteChunk();
                            break;
                        }
                    case ChunkType.MeshSubsets:
                        {
                            ChunkMeshSubsets chkMeshSubsets = new ChunkMeshSubsets();
                            chkMeshSubsets.id = ChkHdr.id;
                            chkMeshSubsets.chunkType = ChkHdr.type;
                            chkMeshSubsets.version = ChkHdr.version;
                            chkMeshSubsets.ReadChunk(cgfReader, ChkHdr.offset);
                            CgfChunks.Add(chkMeshSubsets);
                            ChunkDictionary.Add(chkMeshSubsets.id, chkMeshSubsets);
                            //chkMeshSubsets.WriteChunk();
                            break;
                        }
                    case ChunkType.Node:
                        {
                            ChunkNode chkNode = new ChunkNode();
                            chkNode.ReadChunk(cgfReader, ChkHdr.offset);
                            chkNode.id = ChkHdr.id;
                            chkNode.chunkType = ChkHdr.type;
                            CgfChunks.Add(chkNode);
                            ChunkDictionary.Add(chkNode.id, chkNode);

                            if (RootNodeID == 0x0)
                            {
                                //Console.WriteLine("Found a Parent chunk node.  Adding to the dictionary.");
                                RootNodeID = chkNode.id;
                                RootNode = chkNode;
                                ChunkDictionary[RootNodeID].WriteChunk();
                            }
                            //chkNode.WriteChunk();
                            break;
                        }
                    case ChunkType.CompiledBones:
                        {
                            ChunkCompiledBones chkCompiledBones = new ChunkCompiledBones();
                            chkCompiledBones.ReadChunk(cgfReader, ChkHdr.offset);
                            chkCompiledBones.id = ChkHdr.id;
                            chkCompiledBones.chunkType = ChkHdr.type;
                            CgfChunks.Add(chkCompiledBones);
                            ChunkDictionary.Add(chkCompiledBones.id, chkCompiledBones);
                            break;
                        }
                    case ChunkType.Helper:
                        {
                            ChunkHelper chkHelper = new ChunkHelper();
                            chkHelper.version = ChkHdr.version;
                            chkHelper.chunkType = ChkHdr.type;
                            chkHelper.ReadChunk(cgfReader, ChkHdr.offset);
                            chkHelper.id = ChkHdr.id;
                            CgfChunks.Add(chkHelper);
                            ChunkDictionary.Add(chkHelper.id, chkHelper);
                            //chkHelper.WriteChunk();
                            break;
                        }
                    case ChunkType.Controller:
                        {
                            // Having a problem with this.  If the id is 0x000000ff, it says dup key for the 300i.
                            ChunkController chkController = new ChunkController();
                            chkController.ReadChunk(cgfReader, ChkHdr.offset);
                            chkController.chunkType = ChkHdr.type;
                            chkController.version = ChkHdr.version;
                            chkController.id = ChkHdr.id;
                            chkController.size = ChkHdr.size;
                            CgfChunks.Add(chkController);
                            try
                            {
                                ChunkDictionary.Add(chkController.id, chkController);
                            }
                            catch (ArgumentException)
                            {
                                Console.WriteLine("An element with key {0} already exists.", chkController.id);
                                ChunkDictionary[chkController.id].WriteChunk();
                            }

                            //chkController.WriteChunk();
                            break;
                        }
                    case ChunkType.SceneProps:
                        {
                            ChunkSceneProp chkSceneProp = new ChunkSceneProp();
                            chkSceneProp.ReadChunk(cgfReader, ChkHdr.offset);
                            chkSceneProp.chunkType = ChkHdr.type;
                            chkSceneProp.id = ChkHdr.id;
                            chkSceneProp.size = ChkHdr.size;
                            CgfChunks.Add(chkSceneProp);
                            ChunkDictionary.Add(chkSceneProp.id, chkSceneProp);
                            //chkSceneProp.WriteChunk();
                            break;
                        }
                    case ChunkType.CompiledPhysicalProxies:
                        {
                            ChunkCompiledPhysicalProxies chkCompiledPhysicalProxy = new ChunkCompiledPhysicalProxies();
                            chkCompiledPhysicalProxy.ReadChunk(cgfReader, ChkHdr.offset);
                            chkCompiledPhysicalProxy.chunkType = ChkHdr.type;
                            chkCompiledPhysicalProxy.id = ChkHdr.id;
                            chkCompiledPhysicalProxy.size = ChkHdr.size;
                            CgfChunks.Add(chkCompiledPhysicalProxy);
                            ChunkDictionary.Add(chkCompiledPhysicalProxy.id, chkCompiledPhysicalProxy);
                            //chkCompiledPhysicalProxy.WriteChunk();
                            break;
                        }
                    default:
                        {
                            // If we hit this point, it's an unimplemented chunk and needs to be added.
                            //Console.WriteLine("Chunk type found that didn't match known versions: {0}",ChkHdr.type);
                            break;
                        }
                }
            }

        }
        public void ReadCgfData(FileInfo inputFile, ArgsHandler args)  // Constructor for CgfFormat.  This populates the structure.  Modify to handle .cgam files too.
        {
            // Call GetCgfData on the inputfile in args.  If inputfile2 exists, that is a .cgam file and should be called as well.
            this.Args = args;                            // in the event something else needs to know what was passed
            this.DataFile = args.InputFiles.First(); // TODO: Process more than one file
            this.objOutputFile = args.OutputFile;
            this.GetCgfData(inputFile);
        }
        public Vector3 GetTransform(ChunkNode chunkNode, Vector3 transform)        //  Calculate the transform of a node by getting parent's transform.
        {
            Vector3 resultant = new Vector3();
            Console.WriteLine("Transforming {0}.  ID: {1:X},  Parent ID: {2:X}", chunkNode.Name, chunkNode.id, chunkNode.Parent);
            float x = chunkNode.Transform.m41 / 100;
            float y = chunkNode.Transform.m42 / 100;
            float z = chunkNode.Transform.m43 / 100;
            // if we're at the parent, we just want to return.  Otherwise we want to go up to the next level.
            // Matrix math here.  final x is x*m11 + y*m12 + z*m13.  Same for y and z
            // in some models, there are multiple 0xFFFFFFFF parent node chunks.  This will need to be resolved
            if (chunkNode.Parent != 0xFFFFFFFF)
            {
                resultant.x += x + (transform.x * chunkNode.Transform.m11) + (transform.y * chunkNode.Transform.m12) + (transform.z * chunkNode.Transform.m13);
                resultant.y += y + (transform.x * chunkNode.Transform.m21) + (transform.y * chunkNode.Transform.m22) + (transform.z * chunkNode.Transform.m23);
                resultant.z += z + (transform.x * chunkNode.Transform.m31) + (transform.y * chunkNode.Transform.m32) + (transform.z * chunkNode.Transform.m33);

                transform = GetTransform((ChunkNode)ChunkDictionary[chunkNode.Parent], resultant);
            }
            Console.WriteLine();
            return transform;
        }
        public void WriteTransform(Vector3 transform)
        {
            Console.WriteLine("Transform:");
            Console.WriteLine("{0}    {1}    {2}", transform.x, transform.y, transform.z);
            Console.WriteLine();
        }

        public class Header
        {
            public char[] fileSignature; // The CGF file signature.  CryTek for 3.5, CrChF for 3.6
            public uint fileType; // The CGF file type (geometry or animation)  3.5 only
            public uint chunkVersion; // The version of the chunk table  3.5 only
            public int fileOffset; //Position of the chunk table in the CGF file
            //public uint numChunks; // Number of chunks in the Chunk Table (3.6 only.  3.5 has it in Chunk Table)
            //public int FileVersion;         // 0 will be 3.4 and older, 1 will be 3.6 and newer.  THIS WILL CHANGE
            // methods
            public void GetHeader(BinaryReader binReader)  //constructor with 1 arg
            {
                //Header cgfHeader = new Header();
                // populate the Header objects
                fileSignature = new char[8];
                fileSignature = binReader.ReadChars(8);
                string s = new string(fileSignature);
                Console.Write("fileSignature is {0}, ", s);
                if (s.ToLower().Contains("crytek"))
                {
                    Console.WriteLine("Version 3.4 or earlier");
                    fileType = binReader.ReadUInt32();
                    chunkVersion = binReader.ReadUInt32();
                    fileOffset = binReader.ReadInt32();  // location of the chunk table
                    FileVersion = 0;                     // File version 0 is Cryengine 3.4 and older
                }
                else
                {
                    Console.WriteLine("Crytek Version 3.6 or newer");
                    NumChunks = binReader.ReadUInt32();  // number of Chunks in the chunk table
                    fileOffset = binReader.ReadInt32(); // location of the chunk table
                    FileVersion = 1;                    // File version 1 is Cryengine 3.6 and newer 
                }
                // WriteChunk();
                return;
            }
            public void WriteChunk()  // output header to console for testing
            {
                string tmpFileSig;
                tmpFileSig = new string(fileSignature);
                Console.WriteLine("*** HEADER ***");
                Console.WriteLine("    Header Filesignature: {0}", tmpFileSig);
                if (tmpFileSig.ToLower().Contains("crytek"))
                {
                    Console.WriteLine("    FileType:            {0:X}", fileType);
                    Console.WriteLine("    ChunkVersion:        {0:X}", chunkVersion);
                    Console.WriteLine("    ChunkTableOffset:    {0:X}", fileOffset);
                }
                else
                {
                    Console.WriteLine("    NumChunks:           {0:X}", NumChunks);
                    Console.WriteLine("    ChunktableOffset:    {0:X}", fileOffset);
                }

                Console.WriteLine("*** END HEADER ***");
                return;
            }
        }
        public class ChunkTable  // reads the chunk table into a list of ChunkHeaders
        {
            public List<ChunkHeader> chkHeaders = new List<ChunkHeader>();

            // methods
            public void GetChunkTable(BinaryReader b, int f)
            {
                // need to seek to the start of the table here.  foffset points to the start of the table
                b.BaseStream.Seek(f, 0);
                if (FileVersion == 0)           // old 3.4 format
                {
                    NumChunks = b.ReadUInt32();  // number of Chunks in the table.
                    int i; // counter for loop to read all the chunkHeaders
                    for (i = 0; i < NumChunks; i++)
                    {
                        //Console.WriteLine("Loop {0}", i);
                        ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                        uint headerType = b.ReadUInt32(); // read the value, then parse it
                        tempChkHdr.type = (ChunkType)Enum.ToObject(typeof(ChunkType), headerType);
                        //Console.WriteLine("headerType: '{0}'", tempChkHdr.type);
                        tempChkHdr.version = b.ReadUInt32();
                        tempChkHdr.offset = b.ReadUInt32();
                        tempChkHdr.id = b.ReadUInt32();  // This is the chunk ID (except timing)
                        // hack to fix the timing chunk ID, since we don't want it to conflict.  Add 0xFFFF0000 to it.
                        if ((tempChkHdr.type == ChunkType.Timing) || ((uint)tempChkHdr.type == 0x100E))
                        {
                            tempChkHdr.id = tempChkHdr.id + 0xFFFF0000;
                        }
                        tempChkHdr.size = b.ReadUInt32();

                        chkHeaders.Add(tempChkHdr);
                        //tempChkHdr.WriteChunk();
                    }
                }
                if (FileVersion == 1)           // Newer 3.7+ format.  Only know of Star Citizen using this for now.
                {
                    int i; // counter for loop to read all the chunkHeaders
                    //Console.WriteLine("Numchunks is {0}", NumChunks);
                    for (i = 0; i < NumChunks; i++)
                    {
                        //Console.WriteLine("Loop {0}", i);
                        ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                        //uint headerType = b.ReadUInt32(); // read the value, then parse it
                        UInt16 headerType = b.ReadUInt16();
                        switch (headerType)
                        {
                            case 0x1000: tempChkHdr.type = ChunkType.Mesh;
                                break;
                            case 0x1001: tempChkHdr.type = ChunkType.Helper;
                                break;
                            case 0x1002: tempChkHdr.type = ChunkType.VertAnim;
                                break;
                            case 0x1003: tempChkHdr.type = ChunkType.BoneAnim;
                                break;
                            case 0x1004: tempChkHdr.type = ChunkType.GeomNameList;
                                break;
                            case 0x1005: tempChkHdr.type = ChunkType.BoneNameList;
                                break;
                            case 0x1006: tempChkHdr.type = ChunkType.MtlList;
                                break;
                            case 0x1007: tempChkHdr.type = ChunkType.MRM;
                                break;
                            case 0x1008: tempChkHdr.type = ChunkType.SceneProps;
                                break;
                            case 0x1009: tempChkHdr.type = ChunkType.Light;
                                break;
                            case 0x100A: tempChkHdr.type = ChunkType.PatchMesh;
                                break;
                            case 0x100B: tempChkHdr.type = ChunkType.Node;
                                break;
                            case 0x100C: tempChkHdr.type = ChunkType.Mtl;
                                break;
                            case 0x100D: tempChkHdr.type = ChunkType.Controller;
                                break;
                            case 0x100E: tempChkHdr.type = ChunkType.Timing;
                                break;
                            case 0x100F: tempChkHdr.type = ChunkType.BoneMesh;
                                break;
                            case 0x1010: tempChkHdr.type = ChunkType.BoneLightBinding;
                                break;
                            case 0x1011: tempChkHdr.type = ChunkType.MeshMorphTarget;
                                break;
                            case 0x1012: tempChkHdr.type = ChunkType.BoneInitialPos;
                                break;
                            case 0x1013: tempChkHdr.type = ChunkType.SourceInfo;
                                break;
                            case 0x1014: tempChkHdr.type = ChunkType.MtlName;
                                break;
                            case 0x1015: tempChkHdr.type = ChunkType.ExportFlags;
                                break;
                            case 0x1016: tempChkHdr.type = ChunkType.DataStream;
                                break;
                            case 0x1017: tempChkHdr.type = ChunkType.MeshSubsets;
                                break;
                            case 0x1018: tempChkHdr.type = ChunkType.MeshPhysicsData;
                                break;
                            default:
                                Console.WriteLine("Unknown Chunk Type found {0:X}.  Skipping...", headerType);
                                break;
                        }
                        //tempChkHdr.type36 = (ChunkType36)Enum.ToObject(typeof(ChunkType36), tempChkHdr);
                        //Console.WriteLine("headerType: '{0}'", tempChkHdr.type);
                        tempChkHdr.version = (uint)b.ReadUInt16();
                        tempChkHdr.id = b.ReadUInt32();  // This is the reference number to identify the mesh/datastream
                        tempChkHdr.size = b.ReadUInt32();
                        tempChkHdr.offset = b.ReadUInt32();
                        // hack to fix the timing chunk ID, since we don't want it to conflict.  Add 0xFFFF0000 to it.

                        chkHeaders.Add(tempChkHdr);  // Add it to the list.
                        //tempChkHdr.WriteChunk();
                    }

                }
            }
            public void WriteChunk()
            {
                Console.WriteLine("*** Chunk Header Table***");
                Console.WriteLine("Chunk Type              Version   ID        Size      Offset    ");
                foreach (ChunkHeader chkHdr in chkHeaders)
                {
                    Console.WriteLine("{0,-24}{1,-10:X}{2,-10:X}{3,-10:X}{4,-10:X}", chkHdr.type, chkHdr.version, chkHdr.id, chkHdr.size, chkHdr.offset);
                }
            }
        }
        public class ChunkHeader
        {
            public ChunkType type;
            public uint version;
            public uint offset;
            public uint id;
            public uint size; //  Size of the chunk

            // methods
            public void WriteChunk()  // write the Chunk Header Table to the console.  For testing.
            {
                Console.WriteLine("*** CHUNK HEADER ***");
                Console.WriteLine("    ChunkType: {0}", type);
                Console.WriteLine("    ChunkVersion: {0:X}", version);
                Console.WriteLine("    offset: {0:X}", offset);
                Console.WriteLine("    ID: {0:X}", id);
                Console.WriteLine("*** END CHUNK HEADER ***");
            }
        }

        public class Chunk
        {
            public FileOffset fOffset = new FileOffset();  // starting offset of the chunk.  Int, not uint
            public ChunkType chunkType; // Type of chunk
            public uint version;        // version of this chunk
            public uint id;             // ID of the chunk.
            public uint size;           // size of this chunk in bytes

            public virtual void ReadChunk(BinaryReader b, uint f)
            {
                // Don't do anything.  This is just a placeholder
            }
            public virtual void WriteChunk()
            {
                // Don't do anything.  Placeholder
            }
        }
        public class ChunkHelper : Chunk        // cccc0001:  Helper chunk.  This is the top level, then nodes, then mesh, then mesh subsets
        {
            public string Name;
            public HelperType Type;
            public Vector3 Pos;
            public Matrix44 Transform;
            

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Helper chunk
                if (FileVersion == 0)
                {
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), b.ReadUInt32());
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();
                    id = b.ReadUInt32();
                }
                if (FileVersion == 1)
                {
                    //chunkType = 0;
                }
                Type = (HelperType)Enum.ToObject(typeof(HelperType), b.ReadUInt32());
                if (version == 0x744)  // only has the Position.
                {
                    Pos.x = b.ReadSingle();
                    Pos.y = b.ReadSingle();
                    Pos.z = b.ReadSingle();
                }
                else if (version == 0x362)   // will probably never see these.
                {
                    char[] tmpName = new char[64];
                    tmpName = b.ReadChars(64);
                    int stringLength = 0;
                    for (int i = 0; i < tmpName.Length; i++)
                    {
                        if (tmpName[i] == 0)
                        {
                            stringLength = i;
                            break;
                        }
                    }
                    Name = new string(tmpName, 0, stringLength);
                    Type = (HelperType)Enum.ToObject(typeof(HelperType), b.ReadUInt32());
                    Pos.x = b.ReadSingle();
                    Pos.y = b.ReadSingle();
                    Pos.z = b.ReadSingle();
                }

                // chunkHelper = this;
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START Helper Chunk ***");
                Console.WriteLine("    ChunkType:   {0}", chunkType);
                Console.WriteLine("    Version:     {0:X}", version);
                Console.WriteLine("    ID:          {0:X}", id);
                Console.WriteLine("    HelperType:  {0}", Type);
                Console.WriteLine("    Position:    {0}, {1}, {2}", Pos.x, Pos.y, Pos.z);
                Console.WriteLine("*** END Helper Chunk ***");
            }
        }
        
        public class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
        {
            public uint[] Reserved;             // 8 reserved bytes
            public String RootBoneID;          // Controller ID?  Name?  Not sure yet.
            public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
            public uint NumBones;               // Number of bones in the chunk
            // Bone info
            //public Dictionary<UInt32, CompiledBone> BoneDictionary = new Dictionary<UInt32, CompiledBone>();
            public Dictionary<String, CompiledBone> BoneDictionary = new Dictionary<String, CompiledBone>();  // Name and CompiledBone object

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Node chunk.  f+12(?) will always be the start of this chunk, so us it!
                if (FileVersion == 0)
                {
                    uint tmpNodeChunk = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpNodeChunk);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();
                    id = b.ReadUInt32();
                }
                Reserved = new uint[8];
                for (int i = 0; i < 8; i++)
                {
                    Reserved[i] = b.ReadUInt32();
                }
                //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
                //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.

                GetCompiledBones(b, "isRoot");                        // Start reading at the root bone

            }
            public void GetCompiledBones(BinaryReader b, String parent)        // Recursive call to read the bone at the current seek, and all children.
            {
                // Start reading all the properties of this bone.
                CompiledBone tempBone = new CompiledBone();
                // Console.WriteLine("** Current offset {0:X}", b.BaseStream.Position);
                tempBone.offset = b.BaseStream.Position;
                tempBone.ReadCompiledBone(b);
                tempBone.parentID = parent;
                //tempBone.WriteCompiledBone();
                tempBone.childNames = new String[tempBone.numChildren]; 
                BoneDictionary.Add(tempBone.boneName,tempBone);         // Add this bone to the dictionary.

                for (int i = 0; i < tempBone.numChildren; i++)
                {
                    // If child offset is 1, then we're at the right position anyway.  If it's 2, you want to 584 bytes.  3 is (584*2)...
                    // Move to the offset of child.  If there are no children, we shouldn't move at all.
                    b.BaseStream.Seek(tempBone.offset + 584 * tempBone.offsetChild + (i * 584), 0);  
                    GetCompiledBones(b, tempBone.boneName);
                }
                // Need to set the seek position back to the parent at this point?  Can use parent offset * 584...  Parent offset is a neg number
                //Console.WriteLine("Parent offset: {0}", tempBone.offsetParent);
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START CompiledBone Chunk ***");
                Console.WriteLine("    ChunkType:           {0}", chunkType);
                Console.WriteLine("    Node ID:             {0:X}", id);

            }
        }
        public class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
        {
            // Properties.  VERY similar to datastream, since it's essential vertex info.
            public uint Flags2;
            public uint NumBones; // Number of data entries
            public uint BytesPerElement; // Bytes per data entry
            //public uint Reserved1;
            //public uint Reserved2;
            public HitBox[] HitBoxes;

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Hitbox chunk
                if (FileVersion == 0)
                {
                    uint tmpNodeChunk = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpNodeChunk);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();
                    id = b.ReadUInt32();
                    // Console.WriteLine("Chunk ID is {0:X}", id);
                }

                NumBones = b.ReadUInt32(); // number of Bones in this chunk.
                // Console.WriteLine("Number of bones (hitboxes): {0}", NumBones);
                HitBoxes = new HitBox[NumBones];    // now have an array of hitboxes
                for (int i = 0; i < NumBones; i++)
                {
                    // Start populating the hitbox array
                    HitBoxes[i].ID = b.ReadUInt32();
                    HitBoxes[i].NumVertices = b.ReadUInt32();
                    HitBoxes[i].NumIndices = b.ReadUInt32();
                    HitBoxes[i].Unknown2 = b.ReadUInt32();      // Probably a fill of some sort?
                    HitBoxes[i].Vertices = new Vector3[HitBoxes[i].NumVertices];
                    HitBoxes[i].Indices = new ushort[HitBoxes[i].NumIndices];

                    //Console.WriteLine("Hitbox {0}, {1:X} Vertices and {2:X} Indices", i, HitBoxes[i].NumVertices, HitBoxes[i].NumIndices);
                    for (int j = 0; j < HitBoxes[i].NumVertices; j++)
                    {
                        HitBoxes[i].Vertices[j].x = b.ReadSingle();
                        HitBoxes[i].Vertices[j].y = b.ReadSingle();
                        HitBoxes[i].Vertices[j].z = b.ReadSingle();
                        // Console.WriteLine("{0} {1} {2}",HitBoxes[i].Vertices[j].x,HitBoxes[i].Vertices[j].y,HitBoxes[i].Vertices[j].z);
                    }
                    // Read the indices
                    for (int j = 0; j < HitBoxes[i].NumIndices; j++)
                    {
                        HitBoxes[i].Indices[j] = b.ReadUInt16();
                        //Console.WriteLine("Indices: {0}", HitBoxes[i].Indices[j]);
                    }
                    // Console.WriteLine("Index 0 is {0}, Index 9 is {1}", HitBoxes[i].Indices[0],HitBoxes[i].Indices[9]);
                    // read the crap at the end so we can move on.
                    for (int j = 0; j < HitBoxes[i].Unknown2/2; j++)
                    {
                        b.ReadUInt16();
                    }
                    // HitBoxes[i].WriteHitBox();
                }

            }
            public override void WriteChunk()
            {
                base.WriteChunk();
            }
        }

        public class ChunkNode : Chunk          // cccc000b:   Node
        {
            public string Name;  // String 64.
            public uint Object;  // Mesh or Helper Object chunk ID
            public uint Parent;  // Node parent.  if 0xFFFFFFFF, it's the top node.  Maybe...
            public uint NumChildren;
            public uint MatID;  // reference to the material ID for this Node chunk
            public Boolean IsGroupHead; //
            public Boolean IsGroupMember;
            public byte[] Reserved1; // padding, 2 bytes long... or just read a uint 
            private uint Filler;
            public Matrix44 Transform;   // Transformation matrix
            public Vector3 Pos;  // position vector of above transform
            public Quat Rot;     // rotation component of above transform
            public Vector3 Scale;  // Scalar component of above matrix44
            public uint PosCtrl;  // Position Controller ID (Controller Chunk type)
            public uint RotCtrl;  // Rotation Controller ID 
            public uint SclCtrl;  // Scalar controller ID
            // These are children, materials, etc.
            public ChunkMtlName MaterialChunk;
            public ChunkNode[] NodeChildren;


            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Node chunk
                if (FileVersion == 0)
                {
                    uint tmpNodeChunk = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpNodeChunk);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();
                    id = b.ReadUInt32();
                }
                // Read the Name string
                char[] tmpName = new char[64];
                tmpName = b.ReadChars(64);
                int stringLength = 0;
                for (int i = 0; i < tmpName.Length; i++)
                {
                    if (tmpName[i] == 0)
                    {
                        stringLength = i;
                        break;
                    }
                }
                Name = new string(tmpName, 0, stringLength);
                Object = b.ReadUInt32(); // Object reference ID
                Parent = b.ReadUInt32();
                //Console.WriteLine("Node chunk:  {0}. ", Name);
                if (Parent == 0xFFFFFFFF)
                {
                    Console.WriteLine("Found Parent with 0xFFFFFF.  Name:  {0}", Name);
                }

                NumChildren = b.ReadUInt32();
                MatID = b.ReadUInt32();  // Material ID?
                Filler = b.ReadUInt32();  // Actually a couple of booleans and a padding
                // Read the 4x4 transform matrix.  Should do a couple of for loops, but data structures...
                Transform.m11 = b.ReadSingle();
                Transform.m12 = b.ReadSingle();
                Transform.m13 = b.ReadSingle();
                Transform.m14 = b.ReadSingle();
                Transform.m21 = b.ReadSingle();
                Transform.m22 = b.ReadSingle();
                Transform.m23 = b.ReadSingle();
                Transform.m24 = b.ReadSingle();
                Transform.m31 = b.ReadSingle();
                Transform.m32 = b.ReadSingle();
                Transform.m33 = b.ReadSingle();
                Transform.m34 = b.ReadSingle();
                Transform.m41 = b.ReadSingle();
                Transform.m42 = b.ReadSingle();
                Transform.m43 = b.ReadSingle();
                Transform.m44 = b.ReadSingle();  
                // Read the position Pos Vector3
                Pos.x = b.ReadSingle()/100;
                Pos.y = b.ReadSingle()/100;
                Pos.z = b.ReadSingle()/100;
                // Read the rotation Rot Quad
                Rot.w = b.ReadSingle();
                Rot.x = b.ReadSingle();
                Rot.y = b.ReadSingle();
                Rot.z = b.ReadSingle();
                // Read the Scale Vector 3
                Scale.x = b.ReadSingle();
                Scale.y = b.ReadSingle();
                Scale.z = b.ReadSingle();
                // read the controller pos/rot/scale
                PosCtrl = b.ReadUInt32();
                RotCtrl = b.ReadUInt32();
                SclCtrl = b.ReadUInt32();
                // Good enough for now.
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START Node Chunk ***");
                Console.WriteLine("    ChunkType:           {0}", chunkType);
                Console.WriteLine("    Node ID:             {0:X}", id);
                Console.WriteLine("    Node Name:           {0}", Name);
                Console.WriteLine("    Object ID:           {0:X}", Object);
                Console.WriteLine("    Parent ID:           {0:X}", Parent);
                Console.WriteLine("    Number of Children:  {0}", NumChildren);
                Console.WriteLine("    Material ID:         {0:X}", MatID); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
                Console.WriteLine("    Position:            {0:F7}   {1:F7}   {2:F7}", Pos.x, Pos.y, Pos.z);
                Console.WriteLine("    Scale:               {0:F7}   {1:F7}   {2:F7}", Scale.x, Scale.y, Scale.z);
                Console.WriteLine("    Transformation:      {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m11, Transform.m12, Transform.m13, Transform.m14);
                Console.WriteLine("                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m21, Transform.m22, Transform.m23, Transform.m24);
                Console.WriteLine("                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m31, Transform.m32, Transform.m33, Transform.m34);
                Console.WriteLine("                         {0:F7}  {1:F7}  {2:F7}  {3:F7}", Transform.m41, Transform.m42, Transform.m43, Transform.m44);
                Console.WriteLine("*** END Node Chunk ***");

            }
        }
        public class ChunkController : Chunk    // cccc000d:  Controller chunk
        {
            public CtrlType ControllerType;
            public uint NumKeys;
            public uint ControllerFlags;        // technically a bitstruct to identify a cycle or a loop.
            public uint ControllerID;           // Unique id based on CRC32 of bone name.  Ver 827 only?
            public Key[] Keys;                  // array length NumKeys.  Ver 827?


            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
                if (FileVersion == 0)
                {
                    uint tmpChkType = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChkType);
                    version = b.ReadUInt32();  //0x00000918 is Far Cry, Crysis, MWO, Aion
                    fOffset.Offset = b.ReadInt32();
                    id = b.ReadUInt32();
                }
                //Console.WriteLine("ID is:  {0}", id);
                ControllerType = (CtrlType)Enum.ToObject(typeof(CtrlType), b.ReadUInt32());
                NumKeys = b.ReadUInt32();
                ControllerFlags = b.ReadUInt32();
                ControllerID = b.ReadUInt32();
                Keys = new Key[NumKeys];
                for (int i = 0; i < NumKeys; i++)
                {
                    // Will implement fully later.  Not sure I understand the structure, or if it's necessary.
                    Keys[i].Time = b.ReadInt32();
                    // Console.WriteLine("Time {0}", Keys[i].Time);
                    Keys[i].AbsPos.x = b.ReadSingle();
                    Keys[i].AbsPos.y = b.ReadSingle();
                    Keys[i].AbsPos.z = b.ReadSingle();
                    // Console.WriteLine("Abs Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                    Keys[i].RelPos.x = b.ReadSingle();
                    Keys[i].RelPos.y = b.ReadSingle();
                    Keys[i].RelPos.z = b.ReadSingle();
                    // Console.WriteLine("Rel Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
                }

            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** Controller Chunk ***");
                Console.WriteLine("Version:                 {0:X}", version);
                Console.WriteLine("ID:                      {0:X}", id);
                Console.WriteLine("Number of Keys:          {0}", NumKeys);
                Console.WriteLine("Controller Type:         {0}", ControllerType);
                Console.WriteLine("Conttroller Flags:       {0}", ControllerFlags);
                Console.WriteLine("Controller ID:           {0}", ControllerID);
                for (int i = 0; i < NumKeys; i++)
                {
                    Console.WriteLine("        Key {0}:       Time: {1}", i, Keys[i].Time);
                    Console.WriteLine("        AbsPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                    Console.WriteLine("        RelPos {0}:    {1:F7}, {2:F7}, {3:F7}", i, Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
                }
                
            }

        }
        public class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
        {
            public uint ChunkOffset;  // for some reason the offset of Export Flag chunk is stored here.
            public uint Flags;    // ExportFlags type technically, but it's just 1 value
            public uint Unknown1; // uint, no idea what they are
            public uint[] RCVersion;  // 4 uints
            public char[] RCVersionString;  // Technically String16
            public uint[] Reserved;  // 32 uints
            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
                uint tmpExportFlag = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpExportFlag);
                version = b.ReadUInt32();
                ChunkOffset = b.ReadUInt32();
                id = b.ReadUInt32();
                Unknown1 = b.ReadUInt32();
                RCVersion = new uint[4];
                int count = 0;
                for (count = 0; count < 4; count++)
                {
                    RCVersion[count] = b.ReadUInt32();
                }
                RCVersionString = new char[16];
                RCVersionString = b.ReadChars(16);
                Reserved = new uint[32];
                for (count = 0; count < 4; count++)
                {
                    Reserved[count] = b.ReadUInt32();
                }
                // chunkExportFlags = this;
            }
            public override void WriteChunk()
            {
                string tmpVersionString = new string(RCVersionString);
                Console.WriteLine("*** START EXPORT FLAGS ***");
                Console.WriteLine("    Export Chunk ID: {0:X}", id);
                Console.WriteLine("    ChunkType: {0}", chunkType);
                Console.WriteLine("    Version: {0}", version);
                Console.WriteLine("    Flags: {0}", Flags);
                Console.Write("    RC Version: ");
                for (int i = 0; i < 4; i++)
                {
                    Console.Write(RCVersion[i]);
                }
                Console.WriteLine();
                Console.WriteLine("    RCVersion String: {0}", tmpVersionString);
                Console.WriteLine("    Reserved: {0:X}", Reserved);
                Console.WriteLine("*** END EXPORT FLAGS ***");
            }
        }
        public class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
        {
            public string SourceFile;
            public string Date;
            public string Author;

            public override void ReadChunk(BinaryReader b, uint f)  //
            {
                b.BaseStream.Seek(f, 0);
                chunkType = ChunkType.SourceInfo; // this chunk doesn't actually have the chunktype header.
                // you'd think ReadString() would read from the current offset to the next null byte, but IT DOESN'T.
                int count = 0;                      // read original file
                while (b.ReadChar() != 0)
                {
                    count++;
                } // count now has the null position relative to the seek position
                b.BaseStream.Seek(f, 0);
                char[] tmpSource = new char[count];
                tmpSource = b.ReadChars(count + 1);
                SourceFile = new string(tmpSource);

                count = 0;                          // Read date
                while (b.ReadChar() != 0)
                {
                    count++;
                } // count now has the null position relative to the seek position
                b.BaseStream.Seek(b.BaseStream.Position - count - 1, 0);
                char[] tmpDate = new char[count];
                tmpDate = b.ReadChars(count + 1);  //strip off last 2 characters, because it contains a return
                Date = new string(tmpDate);

                count = 0;                           // Read Author
                while (b.ReadChar() != 0)
                {
                    count++;
                } // count now has the null position relative to the seek position
                b.BaseStream.Seek(b.BaseStream.Position - count - 1, 0);
                char[] tmpAuthor = new char[count];
                tmpAuthor = b.ReadChars(count + 1);
                Author = new string(tmpAuthor);
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** SOURCE INFO CHUNK ***");
                Console.WriteLine("    ID: {0:X}", id);
                Console.WriteLine("    Sourcefile: {0}.  Length {1}", SourceFile, SourceFile.Length);
                Console.WriteLine("    Date:       {0}.  Length {1}", Date, Date.Length);
                Console.WriteLine("    Author:     {0}.  Length {1}", Author, Author.Length);
                Console.WriteLine("*** END SOURCE INFO CHUNK ***");
            }
        }
        public class ChunkMtlName : Chunk  // cccc0014:  provides material name as used in the .mtl file
        {
            // need to find the material ID used by the mesh subsets
            public uint Flags1;  // pointer to the start of this chunk?
            public uint MatType; // for type 800, 0x1 is material library, 0x12 is child, 0x10 is solo material
            public uint Filler2; // for type 800, unknown value
            //public uint NumChildren802; // for type 802, NumChildren
            public uint Filler4; // for type 802, unknown value
            public string Name; // technically a String128 class
            public MtlNamePhysicsType PhysicsType; // enum of a 4 byte uint  For 802 it's an array, 800 a single element.
            public MtlNamePhysicsType[] PhysicsTypeArray; // enum of a 4 byte uint  For 802 it's an array, 800 a single element.
            public uint NumChildren; // number of materials in this name. Max is 66
            // need to implement an array of references here?  Name of Children
            public uint[] Children;
            public uint[] Padding;  // array length of 32
            public uint AdvancedData;  // probably not used
            public float Opacity; // probably not used
            public int[] Reserved;  // array length of 32

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Material Name chunk
                if (FileVersion == 0)
                {
                    uint tmpChunkMtlName = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkMtlName);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();  // offset to this chunk
                    id = b.ReadUInt32();  // ref/chunk number
                }
                // at this point we need to differentiate between Version 800 and 802, since the format differs.
                if (version == 0x800 || version == 0x744)  // guessing on the 744. Aion.
                {
                    MatType = b.ReadUInt32();  // if 0x1, then material lib.  If 0x12, mat name.  This is actually a bitstruct.
                    Filler2 = b.ReadUInt32();
                    // read the material Name, which is a 128 byte char array.  really want it as a string...
                    // long tmpPointer = b.BaseStream.Position;
                    char[] tmpName = new char[128];
                    tmpName = b.ReadChars(128);
                    int stringLength = 0;
                    for (int i = 0; i < tmpName.Length; i++)
                    {
                        if (tmpName[i] == 0)
                        {
                            stringLength = i;
                            break;
                        }
                    }
                    Name = new string(tmpName, 0, stringLength);
                    PhysicsType = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), b.ReadUInt32());
                    NumChildren = b.ReadUInt32();
                    // Now we need to read the Children references.  2 parts; the number of children, and then 66 - numchildren padding
                    Children = new uint[NumChildren];
                    for (int i = 0; i < NumChildren; i++)
                    {
                        Children[i] = b.ReadUInt32();
                    }
                    // Now dump the rest of the padding
                    Padding = new uint[66 - NumChildren];
                    for (int i = 0; i < 66 - NumChildren; i++)
                    {
                        Padding[i] = b.ReadUInt32();
                    }
                }
                if (version == 0x802)
                {
                    // Don't need fillers for this type, but there are no children.
                    Console.WriteLine("version 0x802 material file found....");
                    char[] tmpName = new char[128];
                    tmpName = b.ReadChars(128);
                    int stringLength = 0;
                    for (int i = 0; i < tmpName.Length; i++)
                    {
                        if (tmpName[i] == 0)
                        {
                            stringLength = i;
                            break;
                        }
                    }
                    Name = new string(tmpName, 0, stringLength);
                    NumChildren = b.ReadUInt32();  // number of materials
                    PhysicsTypeArray = new MtlNamePhysicsType[NumChildren];
                    for (int i = 0; i < NumChildren; i++)
                    {
                        PhysicsTypeArray[i] = (MtlNamePhysicsType)Enum.ToObject(typeof(MtlNamePhysicsType), b.ReadUInt32());
                    }
                }
                
                // chunkMtlName = this;
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START MATERIAL NAMES ***");
                Console.WriteLine("    ChunkType:           {0}", chunkType);
                Console.WriteLine("    Material Name:       {0}", Name);
                Console.WriteLine("    Material ID:         {0:X}", id);
                Console.WriteLine("    Version:             {0:X}", version);
                Console.WriteLine("    Number of Children:  {0}", NumChildren);
                Console.WriteLine("    Material Type:       {0:X}", MatType); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
                Console.WriteLine("    Physics Type:        {0}", PhysicsType);
                Console.WriteLine("*** END MATERIAL NAMES ***");
            }
        }
        public class ChunkDataStream : Chunk // cccc0016:  Contains data such as vertices, normals, etc.
        {
            public uint Flags; // not used, but looks like the start of the Data Stream chunk
            public uint Flags1; // not used.  uint after Flags that looks like offsets
            public uint Flags2; // not used, looks almost like a filler.
            public DataStreamType dataStreamType; // type of data (vertices, normals, uv, etc)
            public uint NumElements; // Number of data entries
            public uint BytesPerElement; // Bytes per data entry
            public uint Reserved1;
            public uint Reserved2;
            // Need to be careful with using float for Vertices and normals.  technically it's a floating point of length BytesPerElement.  May need to fix this.
            public Vector3[] Vertices;  // For dataStreamType of 0, length is NumElements. 
            public Vector3[] Normals;   // For dataStreamType of 1, length is NumElements.
            public UV[] UVs;            // for datastreamType of 2, length is NumElements.
            public IRGB[] RGBColors;    // for dataStreamType of 3, length is NumElements.  Bytes per element of 3
            public IRGBA[] RGBAColors;  // for dataStreamType of 4, length is NumElements.  Bytes per element of 4
            public UInt32[] Indices;    // for dataStreamType of 5, length is NumElements.
            // For Tangents on down, this may be a 2 element array.  See line 846+ in cgf.xml
            public Tangent[,] Tangents;  // for dataStreamType of 6, length is NumElements,2.  
            public byte[,] ShCoeffs;     // for dataStreamType of 7, length is NumElement,BytesPerElements.
            public byte[,] ShapeDeformation; // for dataStreamType of 8, length is NumElements,BytesPerElement.
            public byte[,] BoneMap;      // for dataStreamType of 9, length is NumElements,BytesPerElement.
            public byte[,] FaceMap;      // for dataStreamType of 10, length is NumElements,BytesPerElement.
            public byte[,] VertMats;     // for dataStreamType of 11, length is NumElements,BytesPerElement.

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the DataStream chunk
                if (FileVersion == 0) 
                {
                    uint tmpChunkDataStream = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkDataStream);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();  // Offset to this chunk
                    id = b.ReadUInt32();  // Reference to the data stream type.
                }
                Flags2 = b.ReadUInt32(); // another filler
                uint tmpdataStreamType = b.ReadUInt32();
                dataStreamType = (DataStreamType)Enum.ToObject(typeof(DataStreamType), tmpdataStreamType);
                NumElements = b.ReadUInt32(); // number of elements in this chunk

                if (FileVersion == 0)
                {
                    BytesPerElement = b.ReadUInt32(); // bytes per element
                }
                if (FileVersion == 1)
                {
                    BytesPerElement = (UInt32)b.ReadInt16();        // Star Citizen 2.0 is using an int16 here now.
                    b.ReadInt16();                                  // unknown value.   Doesn't look like padding though.
                }

                Reserved1 = b.ReadUInt32();
                Reserved2 = b.ReadUInt32();
                // Now do loops to read for each of the different Data Stream Types.  If vertices, need to populate Vector3s for example.
                switch (dataStreamType)
                {
                    case DataStreamType.VERTICES:  // Ref is 0x00000000
                        {
                            Vertices = new Vector3[NumElements];
                            if (BytesPerElement == 12)
                            {
                                for (int i = 0; i < NumElements; i++)
                                {
                                    Vertices[i].x = b.ReadSingle();
                                    Vertices[i].y = b.ReadSingle();
                                    Vertices[i].z = b.ReadSingle();
                                }
                            }
                            if (BytesPerElement == 8)  // Old Star Citizen files
                            {
                                // 2 byte floats.  Use the Half structure from TK.Math
                                for (int i = 0; i < NumElements; i++)
                                {
                                    //Single flx = new Single();
                                    Half xshort = new Half();
                                    xshort.bits = b.ReadUInt16();
                                    //flx = (Single) b.ReadUInt16();
                                    Vertices[i].x = xshort.ToSingle();

                                    Half yshort = new Half();
                                    yshort.bits = b.ReadUInt16();
                                    Vertices[i].y = yshort.ToSingle();

                                    Half zshort = new Half();
                                    zshort.bits = b.ReadUInt16();
                                    Vertices[i].z = zshort.ToSingle();

                                    short w = b.ReadInt16();  // dump this as not needed.  Last 2 bytes are surplus...sort of.
                                    if (i < 20)
                                    {
                                        //Console.WriteLine("{0} {1} {2} {3}", i, Vertices[i].x, Vertices[i].y, Vertices[i].z);
                                    }
                                }

                            }
                            if (BytesPerElement == 16)  // new Star Citizen files
                            {
                                for (int i = 0; i < NumElements; i++)
                                {
                                    Vertices[i].x = b.ReadSingle();
                                    Vertices[i].y = b.ReadSingle();
                                    Vertices[i].z = b.ReadSingle();
                                    float dump = b.ReadSingle();        // Sometimes there's a W to these structures.  Will investigate.
                                    if (i < 20)
                                    {
                                        //Console.WriteLine("{0} {1} {2} {3}", i, Vertices[i].x, Vertices[i].y, Vertices[i].z);
                                    }
                                }
                            }
                            // Console.WriteLine("{0} elements read", VertexList.Length);
                            // Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;
                        }
                    case DataStreamType.INDICES:  // Ref is 
                        {
                            Indices = new UInt32[NumElements];
                            if (BytesPerElement == 2)
                            {
                                for (int i = 0; i < NumElements; i++)
                                {
                                    Indices[i] = (UInt32)b.ReadUInt16();
                                }
                            }
                            if (BytesPerElement == 4)
                            {
                                for (int i = 0; i < NumElements; i++)
                                {
                                    Indices[i] = b.ReadUInt32();
                                }
                            }
                            //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;
                        }
                    case DataStreamType.NORMALS:
                        {
                            Normals = new Vector3[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                Normals[i].x = b.ReadSingle();
                                Normals[i].y = b.ReadSingle();
                                Normals[i].z = b.ReadSingle();
                                // Console.WriteLine("{0}  {1}  {2}", Normals[i].x, Normals[i].y, Normals[i].z);
                            }
                            //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;

                        }
                    case DataStreamType.UVS:
                        {
                            UVs = new UV[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                UVs[i].U = b.ReadSingle();
                                UVs[i].V = b.ReadSingle();
                                // Console.WriteLine("{0}   {1}", UVs[i].U, UVs[i].V);
                            }
                            //Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;
                        }
                    case DataStreamType.TANGENTS:
                        {
                            Tangents = new Tangent[NumElements, 2];
                            for (int i = 0; i < NumElements; i++)
                            {
                                // These have to be divided by 32767 to be used properly (value between 0 and 1)
                                Tangents[i, 0].x = b.ReadInt16();
                                Tangents[i, 0].y = b.ReadInt16();
                                Tangents[i, 0].z = b.ReadInt16();
                                Tangents[i, 0].w = b.ReadInt16();
                                Tangents[i, 1].x = b.ReadInt16();
                                Tangents[i, 1].y = b.ReadInt16();
                                Tangents[i, 1].z = b.ReadInt16();
                                Tangents[i, 1].w = b.ReadInt16();
                                //Console.WriteLine("{0} {1} {2} {3}", Tangents[i, 0].x, Tangents[i, 0].y, Tangents[i, 0].z, Tangents[i, 0].w);
                            }
                            // Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;
                        }
                    case DataStreamType.COLORS:
                        {
                            if (BytesPerElement == 3)
                            {
                                RGBColors = new IRGB[NumElements];
                                for (int i = 0; i < NumElements; i++)
                                {
                                    RGBColors[i].r = b.ReadByte();
                                    RGBColors[i].g = b.ReadByte();
                                    RGBColors[i].b = b.ReadByte();
                                }
                            }
                            if (BytesPerElement == 4)
                            {
                                RGBAColors = new IRGBA[NumElements];
                                for (int i = 0; i < NumElements; i++)
                                {
                                    RGBAColors[i].r = b.ReadByte();
                                    RGBAColors[i].g = b.ReadByte();
                                    RGBAColors[i].b = b.ReadByte();
                                    RGBAColors[i].a = b.ReadByte();

                                }
                            }
                            break;
                        }
                    case DataStreamType.VERTSUVS:  // 3 half floats for verts, 6 unknown, 2 half floats for UVs
                        {
                            // Console.WriteLine("In VertsUVs...");
                            Vertices = new Vector3[NumElements]; 
                            Normals = new Vector3[NumElements];
                            UVs = new UV[NumElements];
                            if (BytesPerElement == 16)  // new Star Citizen files
                            {
                                for (int i = 0; i < NumElements; i++)
                                {
                                    //Single flx = new Single();
                                    /*float flx = (Single) b.ReadSingle();
                                    Vertices[i].x = flx;
                                    float fly = (Single)b.ReadSingle();
                                    Vertices[i].y = fly;
                                    float flz = (Single)b.ReadSingle();
                                    Vertices[i].z = flz;*/
                                    Half xshort = new Half();
                                    xshort.bits = b.ReadUInt16();
                                    Vertices[i].x = xshort.ToSingle();

                                    Half yshort = new Half();
                                    yshort.bits = b.ReadUInt16();
                                    Vertices[i].y = yshort.ToSingle();

                                    Half zshort = new Half();
                                    zshort.bits = b.ReadUInt16();
                                    Vertices[i].z = zshort.ToSingle();

                                    Half xnorm = new Half();
                                    xnorm.bits = b.ReadUInt16();
                                    Normals[i].x = xnorm.ToSingle();

                                    Half ynorm = new Half();
                                    ynorm.bits = b.ReadUInt16();
                                    Normals[i].y = ynorm.ToSingle();

                                    Half znorm = new Half();
                                    znorm.bits = b.ReadUInt16();
                                    Normals[i].z = znorm.ToSingle();

                                    Half uvu = new Half();
                                    uvu.bits = b.ReadUInt16();
                                    UVs[i].U = uvu.ToSingle();

                                    Half uvv = new Half();
                                    uvv.bits = b.ReadUInt16();
                                    UVs[i].V = uvv.ToSingle();

                                    //short w = b.ReadInt16();  // dump this as not needed.  Last 2 bytes are surplus...sort of.
                                    //if (i < 20)
                                    //{
                                    //    Console.WriteLine("{0:F7} {1:F7} {2:F7} {3:F7} {4:F7}",
                                    //        Vertices[i].x, Vertices[i].y, Vertices[i].z,
                                    //        UVs[i].U, UVs[i].V);
                                    //}
                                }
                            }
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("***** Unknown DataStream Type *****");
                            break;
                        }
                }
                //chunkDataStream = this;
            }
            public override void WriteChunk()
            {
                //string tmpDataStream = new string(Name);
                Console.WriteLine("*** START DATASTREAM ***");
                Console.WriteLine("    ChunkType:                       {0}", chunkType);
                Console.WriteLine("    Version:                         {0:X}", version);
                Console.WriteLine("    DataStream chunk starting point: {0:X}", Flags);
                Console.WriteLine("    Chunk ID:                        {0:X}", id);
                Console.WriteLine("    DataStreamType:                  {0}", dataStreamType);
                Console.WriteLine("    Number of Elements:              {0}", NumElements);
                Console.WriteLine("    Bytes per Element:               {0}", BytesPerElement);
                Console.WriteLine("*** END DATASTREAM ***");

            }
        }
        public class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
        {
            public uint Flags; // probably the offset
            public uint NumMeshSubset; // number of mesh subsets
            public int Reserved1;
            public int Reserved2;
            public int Reserved3;
            public MeshSubset[] MeshSubsets;

            public override void ReadChunk(BinaryReader b, uint f)
            {
                if (FileVersion == 0)
                {
                    b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
                    uint tmpChunkType = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
                    version = b.ReadUInt32(); // probably 800
                    fOffset.Offset = b.ReadInt32();  // offset to this chunk
                    id = b.ReadUInt32(); // ID of this chunk.  Used to reference the mesh chunk
                    Flags = b.ReadUInt32();   // Might be a ref to this chunk
                    NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
                    Reserved1 = b.ReadInt32();
                    Reserved2 = b.ReadInt32();
                    // Reserved3 = b.ReadInt32();
                    MeshSubsets = new MeshSubset[NumMeshSubset];
                    for (int i = 0; i < NumMeshSubset; i++)
                    {
                        MeshSubsets[i].FirstIndex = b.ReadUInt32();
                        MeshSubsets[i].NumIndices = b.ReadUInt32();
                        MeshSubsets[i].FirstVertex = b.ReadUInt32();
                        MeshSubsets[i].NumVertices = b.ReadUInt32();
                        MeshSubsets[i].MatID = b.ReadUInt32();
                        MeshSubsets[i].Radius = b.ReadSingle();
                        MeshSubsets[i].Center.x = b.ReadSingle();
                        MeshSubsets[i].Center.y = b.ReadSingle();
                        MeshSubsets[i].Center.z = b.ReadSingle();
                    }
                }
                if (FileVersion == 1)  // 3.6 and newer files
                {
                    b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
                    Flags = b.ReadUInt32();   // Might be a ref to this chunk
                    NumMeshSubset = b.ReadUInt32();  // number of mesh subsets
                    Reserved1 = b.ReadInt32();
                    Reserved2 = b.ReadInt32();
                    MeshSubsets = new MeshSubset[NumMeshSubset];
                    for (int i = 0; i < NumMeshSubset; i++)
                    {
                        MeshSubsets[i].FirstIndex = b.ReadUInt32();
                        MeshSubsets[i].NumIndices = b.ReadUInt32();
                        MeshSubsets[i].FirstVertex = b.ReadUInt32();
                        MeshSubsets[i].NumVertices = b.ReadUInt32();
                        MeshSubsets[i].MatID = b.ReadUInt32();
                        MeshSubsets[i].Radius = b.ReadSingle();
                        MeshSubsets[i].Center.x = b.ReadSingle();
                        MeshSubsets[i].Center.y = b.ReadSingle();
                        MeshSubsets[i].Center.z = b.ReadSingle();
                    }
                }
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START MESH SUBSET CHUNK ***");
                Console.WriteLine("    ChunkType:       {0}", chunkType);
                Console.WriteLine("    Mesh SubSet ID:  {0:X}", id);
                Console.WriteLine("    Number of Mesh Subsets: {0}", NumMeshSubset);
                for (int i = 0; i < NumMeshSubset; i++)
                {
                    Console.WriteLine("        ** Mesh Subset:          {0}", i);
                    Console.WriteLine("           First Index:          {0}", MeshSubsets[i].FirstIndex);
                    Console.WriteLine("           Number of Indices:    {0}", MeshSubsets[i].NumIndices);
                    Console.WriteLine("           First Vertex:         {0}", MeshSubsets[i].FirstVertex);
                    Console.WriteLine("           Number of Vertices:   {0}  (next will be {1})", MeshSubsets[i].NumVertices, MeshSubsets[i].NumVertices+MeshSubsets[i].FirstVertex);
                    Console.WriteLine("           Material ID:          {0}", MeshSubsets[i].MatID);
                    Console.WriteLine("           Radius:               {0}", MeshSubsets[i].Radius);
                    Console.WriteLine("           Center:   {0},{1},{2}", MeshSubsets[i].Center.x, MeshSubsets[i].Center.y, MeshSubsets[i].Center.z);
                    Console.WriteLine("        ** Mesh Subset {0} End", i);
                }
                Console.WriteLine("*** END MESH SUBSET CHUNK ***");
            }
        }
        public class ChunkMesh : Chunk      //  cccc0000:  Object that points to the datastream chunk.
        {
            // public uint Version;  // 623 Far Cry, 744 Far Cry, Aion, 800 Crysis
            //public bool HasVertexWeights; // for 744
            //public bool HasVertexColors; // 744
            //public bool InWorldSpace; // 623
            //public byte Reserved1;  // padding byte, 744
            //public byte Reserved2;  // padding byte, 744
            public uint Flags1;  // 800  Offset of this chunk. 
            // public uint ID;  // 800  Chunk ID
            public uint Unknown1; // for 800, not sure what this is.  Value is 2?
            public uint Unknown2; // for 800, not sure what this is.  Value is 0?
            public uint NumVertices; // 
            public uint NumIndices;  // Number of indices (each triangle has 3 indices, so this is the number of triangles times 3).
            //public uint NumUVs; // 744
            //public uint NumFaces; // 744
            // Pointers to various Chunk types
            //public ChunkMtlName Material; // 623, Material Chunk, never encountered?
            public uint Unknown3;       // for type 800, not sure what this is.
            public uint NumVertSubsets; // 801, Number of vert subsets
            public uint MeshSubsets; // 800  Reference of the mesh subsets
            // public ChunkVertAnim VertAnims; // 744.  not implemented
            //public Vertex[] Vertices; // 744.  not implemented
            //public Face[,] Faces; // 744.  Not implemented
            //public UV[] UVs; // 744 Not implemented
            //public UVFace[] UVFaces; // 744 not implemented
            // public VertexWeight[] VertexWeights; // 744 not implemented
            //public IRGB[] VertexColors; // 744 not implemented
            public uint UnknownData; // 0x00 word here for some reason.
            public uint VerticesData; // 800, 801.  Need an array because some 801 files have NumVertSubsets
            public uint NumBuffs;
            public uint[] Buffer;       // 801.  For some reason there is a weird buffer here.
            public uint NormalsData; // 800
            public uint UVsData; // 800
            public uint ColorsData; // 800
            public uint Colors2Data; // 800 
            public uint IndicesData; // 800
            public uint TangentsData; // 800
            public uint ShCoeffsData; // 800
            public uint ShapeDeformationData; //800
            public uint BoneMapData; //800
            public uint FaceMapData; // 800
            public uint VertMatsData; // 800
            public uint MeshPhysicsData; // 801
            public uint VertsUVsData;    // 801
            public uint[] ReservedData = new uint[4]; // 800 Length 4
            public uint[] PhysicsData = new uint[4]; // 800
            public Vector3 MinBound; // 800 minimum coordinate values
            public Vector3 MaxBound; // 800 Max coord values
            public uint[] Reserved3 = new uint[32]; // 800 array of 32 uint values.

            //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
                if (FileVersion == 0)
                {
                    uint tmpChunkType = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
                    version = b.ReadUInt32();
                    Flags1 = b.ReadUInt32();  // offset
                    id = b.ReadUInt32();  // Chunk ID  0x23 for candle
                }
                if (version == 0x800)
                {
                    NumVertSubsets = 1;
                    Unknown1 = b.ReadUInt32();  // unknown
                    Unknown1 = b.ReadUInt32();  // unknown
                    NumVertices = b.ReadUInt32();
                    NumIndices = b.ReadUInt32();   //  Number of indices
                    Unknown3 = b.ReadUInt32();
                    MeshSubsets = b.ReadUInt32();  // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
                    UnknownData = b.ReadUInt32();
                    VerticesData = b.ReadUInt32();  // ID of the datastream for the vertices for this mesh
                    NormalsData = b.ReadUInt32();   // ID of the datastream for the normals for this mesh
                    UVsData = b.ReadUInt32();     // refers to the ID in the Normals datastream?
                    ColorsData = b.ReadUInt32();
                    Colors2Data = b.ReadUInt32();
                    IndicesData = b.ReadUInt32();
                    TangentsData = b.ReadUInt32();
                    ShCoeffsData = b.ReadUInt32();
                    ShapeDeformationData = b.ReadUInt32();
                    BoneMapData = b.ReadUInt32();
                    FaceMapData = b.ReadUInt32();
                    VertMatsData = b.ReadUInt32();
                    for (int i = 0; i < 4; i++)
                    {
                        ReservedData[i] = b.ReadUInt32();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        PhysicsData[i] = b.ReadUInt32();
                    }
                    MinBound.x = b.ReadSingle();
                    MinBound.y = b.ReadSingle();
                    MinBound.z = b.ReadSingle();
                    MaxBound.x = b.ReadSingle();
                    MaxBound.y = b.ReadSingle();
                    MaxBound.z = b.ReadSingle();
                    // Not going to read the Reserved 32 element array.
                }
                else if (version == 0x801)
                {
                    Unknown1 = b.ReadUInt32();  // unknown
                    Unknown1 = b.ReadUInt32();  // unknown
                    NumVertices = b.ReadUInt32();
                    NumIndices = b.ReadUInt32();   //
                    //NumBuffs = b.ReadUInt32();
                    //Buffer = new uint[NumBuffs];
                    uint dump = b.ReadUInt32();
                    MeshSubsets = b.ReadUInt32();  // refers to ID in mesh subsets 
                    dump = b.ReadUInt32(); 
                    VerticesData = b.ReadUInt32();
                    NormalsData = b.ReadUInt32();           // ID of the datastream for the normals for this mesh
                    UVsData = b.ReadUInt32();               // refers to the ID in the Normals datastream
                    ColorsData = b.ReadUInt32();
                    Colors2Data = b.ReadUInt32();
                    IndicesData = b.ReadUInt32();
                    TangentsData = b.ReadUInt32();
                    for (int i = 0; i < 4; i++)
                    {
                        ReservedData[i] = b.ReadUInt32();
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        PhysicsData[i] = b.ReadUInt32();
                    }
                    VertsUVsData = b.ReadUInt32();  // This should be a vertsUV index number, not vertices.  Vertices are above.
                    ShCoeffsData = b.ReadUInt32();
                    ShapeDeformationData = b.ReadUInt32();
                    BoneMapData = b.ReadUInt32();
                    FaceMapData = b.ReadUInt32();
                    MinBound.x = b.ReadSingle();
                    MinBound.y = b.ReadSingle();
                    MinBound.z = b.ReadSingle();
                    MaxBound.x = b.ReadSingle();
                    MaxBound.y = b.ReadSingle();
                    MaxBound.z = b.ReadSingle();
                    // not reading the rest
                }
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START MESH CHUNK ***");
                Console.WriteLine("    ChunkType:           {0}", chunkType);
                Console.WriteLine("    Chunk ID:            {0:X}", id);
                Console.WriteLine("    MeshSubSetID:        {0:X}", MeshSubsets);
                Console.WriteLine("    Vertex Datastream:   {0:X}", VerticesData);
                Console.WriteLine("    Normals Datastream:  {0:X}", NormalsData);
                Console.WriteLine("    UVs Datastream:      {0:X}", UVsData);
                Console.WriteLine("    Indices Datastream:  {0:X}", IndicesData);
                Console.WriteLine("    Tangents Datastream: {0:X}", TangentsData);
                Console.WriteLine("    Mesh Physics Data:   {0:X}", MeshPhysicsData);
                Console.WriteLine("    VertUVs:             {0:X}", VertsUVsData);
                Console.WriteLine("    MinBound:            {0:F7}, {1:F7}, {2:F7}", MinBound.x, MinBound.y, MinBound.z);
                Console.WriteLine("    MaxBound:            {0:F7}, {1:F7}, {2:F7}", MaxBound.x, MaxBound.y, MaxBound.z);
                Console.WriteLine("*** END MESH CHUNK ***");
            }
        }
        public class ChunkSceneProp : Chunk     // cccc0008 
        {
            // This chunk isn't really used, but contains some data probably necessary for the game.
            // Size for 0x744 type is always 0xBB4 (test this)
            public UInt32 numProps;             // number of elements in the props array  (31 for type 0x744)
            public String[] prop;
            public String[] propvalue;
            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
                if (FileVersion == 0)
                {
                    uint tmpChunkType = b.ReadUInt32();
                    chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
                    version = b.ReadUInt32();
                    fOffset.Offset = b.ReadInt32();  // offset
                    id = b.ReadUInt32();
                }
                numProps = b.ReadUInt32();          // Should be 31 for 0x744
                prop = new String[numProps];
                propvalue = new String[numProps];

                // Read the array of scene props and their associated values
                for (int i = 0; i < numProps; i++)
                {
                    char[] tmpProp = new char[32];
                    char[] tmpPropValue = new char[64];
                    tmpProp = b.ReadChars(32);
                    int stringLength = 0;
                    for (int j = 0; j < tmpProp.Length; j++)
                    {
                        if (tmpProp[j] == 0)
                        {
                            stringLength = j;
                            break;
                        }
                    }
                    prop[i] = new string(tmpProp, 0, stringLength);

                    tmpPropValue = b.ReadChars(64);
                    stringLength = 0;
                    for (int j = 0; j < tmpPropValue.Length; j++)
                    {
                        if (tmpPropValue[j] == 0)
                        {
                            stringLength = j;
                            break;
                        }
                    }
                    propvalue[i] = new string(tmpPropValue, 0, stringLength);
                }
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** START SceneProp Chunk ***");
                Console.WriteLine("    ChunkType:   {0}", chunkType);
                Console.WriteLine("    Version:     {0:X}", version);
                Console.WriteLine("    ID:          {0:X}", id);
                for (int i = 0; i < numProps; i++)
                {
                    Console.WriteLine("{0,30}{1,20}", prop[i], propvalue[i]);
                }
                Console.WriteLine("*** END SceneProp Chunk ***");
            }
        }
        public class ChunkTimingFormat : Chunk  // cccc000e:  Timing format chunk
        {
            // This chunk doesn't have an ID, although one may be assigned in the chunk table.
            public float SecsPerTick;
            public int TicksPerFrame;
            public uint Unknown1; // 4 bytes, not sure what they are
            public uint Unknown2; // 4 bytes, not sure what they are
            public RangeEntity GlobalRange;
            public int NumSubRanges;

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
                uint tmpChkType = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChkType);
                version = b.ReadUInt32();  //0x00000918 is Far Cry, Crysis, MWO, Aion, SC
                SecsPerTick = b.ReadSingle();
                TicksPerFrame = b.ReadInt32();
                Unknown1 = b.ReadUInt32();
                Unknown2 = b.ReadUInt32();
                GlobalRange.Name = new char[32];
                GlobalRange.Name = b.ReadChars(32);  // Name is technically a String32, but F those structs
                GlobalRange.Start = b.ReadInt32();
                GlobalRange.End = b.ReadInt32();
            }
            public override void WriteChunk()
            {
                string tmpName = new string(GlobalRange.Name);
                Console.WriteLine("*** TIMING CHUNK ***");
                Console.WriteLine("    ID: {0:X}", id);
                Console.WriteLine("    Version: {0:X}", version);
                Console.WriteLine("    Secs Per Tick: {0}", SecsPerTick);
                Console.WriteLine("    Ticks Per Frame: {0}", TicksPerFrame);
                Console.WriteLine("    Global Range:  Name: {0}", tmpName);
                Console.WriteLine("    Global Range:  Start: {0}", GlobalRange.Start);
                Console.WriteLine("    Global Range:  End:  {0}", GlobalRange.End);
                Console.WriteLine("*** END TIMING CHUNK ***");
            }
        }

        public class FileSignature          // NYI. The signature that Cryengine files start with.  Crytek or CrChF 
        {
            public String Read(BinaryReader b)  // Checks the signature
            {
                char[] signature = new char[8];  // first 8 bytes are the file signature.
                signature = b.ReadChars(8);
                string s = new string(signature);
                return s;
            }
        }

        //public ChunkType GetChunkType(uint type)
        //{

        //}          // This is to replace the ChunkType enum, since we have to support both 3.4 and 3.6+
    }

    // Aliases
    public class FileOffset
    { 
        public int Offset;
    } // move this to Structs

    // classes (aka anything more complicated than a fixed size struct, with methods etc.)
    public class ArgsHandler
    {
        /// <summary>
        /// File extensions processed by the handler
        /// </summary>
        private static HashSet<String> _validExtensions = new HashSet<String>
        {
            ".cgf",
            ".cga",
            ".skin",
        };

        /// <summary>
        /// Reverse UVs
        /// </summary>
        public Boolean FlipUVs { get; set; }
        /// <summary>
        /// File to render to
        /// </summary>
        public FileInfo OutputFile { get; set; }
        /// <summary>
        /// Render Wavefront format files
        /// </summary>
        public Boolean Obj { get; set; }
        /// <summary>
        /// Render Blender format files
        /// </summary>
        public Boolean Blend { get; set; }
        /// <summary>
        /// Render COLLADA format files
        /// </summary>
        public Boolean COLLADA { get; set; }
        /// <summary>
        /// Location of the Object Files
        /// </summary>
        public DirectoryInfo ObjectDir { get; set; }
        /// <summary>
        /// Flag used to pass exceptions to installed debuggers
        /// </summary>
        public Boolean Throw { get; set; }

        /// <summary>
        /// List of files to process
        /// </summary>
        public List<FileInfo> InputFiles { get; set; }

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <param name="inputArgs">list of arguments to parse</param>
        /// <returns>0 on success, 1 if anything went wrong</returns>
        public Int32 ProcessArgs(String[] inputArgs)
        {
            this.InputFiles = new List<FileInfo> { };

            Boolean inputFileMode = true;

            for (int i = 0; i < inputArgs.Length; i++)
            {
                #region Parse Input Files

                if (inputFileMode)
                {
                    FileInfo newFile = new FileInfo(inputArgs[i]);

                    if (newFile.Exists)
                    {
                        // Validate file extension - handles .cgam
                        if (!ArgsHandler._validExtensions.Contains(newFile.Extension))
                        {
                            Console.WriteLine("Warning: Unsupported file extension - please use a cga or cgf file");
                            return 1;
                        }

                        // Add to list of files to process
                        this.InputFiles.Add(newFile);

                        // Output file is based on first file name
                        if (i == 0) this.OutputFile = new FileInfo(Path.GetFileNameWithoutExtension(newFile.FullName) + "_out.obj");

                        #region .cgam Auto-Detection

                        FileInfo cgamFile = new FileInfo(Path.ChangeExtension(inputArgs[i], ".cgam"));
                                    
                        if (cgamFile.Exists)
                        {
                            Console.WriteLine("Found cgam file {0}", cgamFile.Name);

                            // Add to list of files to process
                            this.InputFiles.Add(cgamFile);
                        }

                        #endregion

                    }
                    else
                    {
                        inputFileMode = false;
                    }
                }

                #endregion

                #region Parse Other Arguments

                if (!inputFileMode)
                {
                    switch (inputArgs[i].ToLowerInvariant())
                    {
                        #region case "-objectdir"...

                        // Next item in list will be the Object directory
                        case "-objectdir":
                            if (++i > inputArgs.Length)
                            {
                                this.PrintUsage();
                                return 1;
                            }

                            this.ObjectDir = new DirectoryInfo(inputArgs[i]);

                            if (!this.ObjectDir.Exists)
                                throw new DirectoryNotFoundException("Object Directory not found");

                            Console.WriteLine("Object directory set to {0}", inputArgs[i]);

                            break;

                        #endregion
                        #region case "-outfile" / "-outputfile"...

                        // Next item in list will be the output filename
                        case "-outfile":
                        case "-outputfile":
                            if (++i > inputArgs.Length)
                            {
                                this.PrintUsage();
                                return 1;
                            }

                            this.OutputFile = new FileInfo(inputArgs[i]);

                            Console.WriteLine("Output file set to {0}", inputArgs[i]);

                            break;

                        #endregion
                        #region case "-usage"...

                        case "-usage":
                            this.PrintUsage();
                            return 1;

                        #endregion
                        #region case "-flipuv"...

                        case "-flipuv":
                            Console.WriteLine("Flipping UVs.");
                            this.FlipUVs = true;

                            break;

                        #endregion
                        #region case "-blend" / "-blender"...

                        case "-blend":
                        case "-blender":
                            Console.WriteLine("Output format set to Blender (.blend)");
                            this.Blend = true;

                            throw new NotImplementedException();

                            break;

                        #endregion
                        #region case "-obj" / "-object" / "wavefront"...

                        case "-obj":
                        case "-object":
                        case "-wavefront":
                            Console.WriteLine("Output format set to Wavefront (.obj)");
                            this.Obj = true;

                            break;

                        #endregion
                        #region case "-dae" / "-collada"...
                        case "-dae":
                        case "-collada":
                            Console.WriteLine("Output format set to COLLADA (.dae)");
                            this.COLLADA = true;

                            throw new NotImplementedException();

                            break;

                        #endregion
                        #region case "-throw"...

                        case "-throw":
                            Console.WriteLine("Exceptions thrown to debugger");
                            this.Throw = true;

                            break;

                        #endregion
                    }
                }

                #endregion
            }

            // Ensure we have files to process
            if (this.InputFiles.Count == 0)
            {
                this.PrintUsage();
                return 1;
            }

            // Default to Wavefront (.obj) format
            if (!this.Blend && !this.COLLADA && !this.Obj)
                this.Obj = true;

            return 0;
        }

        /// <summary>
        /// Print the usage syntax of the executable
        /// </summary>
        public void PrintUsage()
        {
            var usage = new StringBuilder();
            Console.WriteLine();
            Console.WriteLine("cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-flipUVs] [-throw]");
            Console.WriteLine();
            Console.WriteLine("-usage:           Prints out the usage statement");
            Console.WriteLine();
            Console.WriteLine("<.cgf file>:      Mandatory.  The name of the .cgf, .cga or .skin file to process");
            Console.WriteLine("-output file:     The name of the file to write the output.  Default is <cgf File>.obj.");
            Console.WriteLine("-objectdir:       The name where the base Objects directory is located.  Used to read mtl file.");
            Console.WriteLine("                  Defaults to current directory.");
            Console.WriteLine("-obj:             Export Wavefront format files (Default: true)");
            Console.WriteLine("-blend:           Export Blender format files (Not Implemented)");
            Console.WriteLine("-dae:             Export Collada format files (Not Implemented)");
            Console.WriteLine("-flipUVs:         Flip the UVs");
            Console.WriteLine();
            Console.WriteLine("-throw:           Throw Exceptions to installed debugger");
            Console.WriteLine();
        }
        
        /// <summary>
        /// Print the current arguments of the executable
        /// </summary>
        public void WriteArgs()
        {
            Console.WriteLine();
            Console.WriteLine("*** Submitted args ***");
            Console.WriteLine("    Input files:            {0}", this.InputFiles.First().Name);
            for (Int32 i = 1, j = this.InputFiles.Count; i < j; i++)
            {
                Console.WriteLine("                            {0}", this.InputFiles[i].Name);
            }
            if (this.ObjectDir != null)
            {
                Console.WriteLine("    Object dir:             {0}", this.ObjectDir.FullName);
            }
            if (this.OutputFile != null)
            {
                Console.WriteLine("    Output file:            {0}", this.OutputFile.FullName);
            }
            Console.WriteLine("    Flip UVs:               {0}", this.FlipUVs);
            Console.WriteLine("    Output to .obj:         {0}", this.Obj);
            Console.WriteLine("    Output to .blend:       {0}", this.Blend);
            Console.WriteLine("    Output to .dae:         {0}", this.COLLADA); 
            Console.WriteLine();
        }
    }

    class Program
    {
        static Int32 Main(String[] args)
        {
            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

            try
            {
                if (result == 0)
                {
                    #region Process Input Files

                    List<CgfData> cgfData = new List<CgfData> { };

                    for (Int32 i = 0, j = argsHandler.InputFiles.Count; i < j; i++)
                    {
                        var data = new CgfData { };

                        data.ReadCgfData(argsHandler.InputFiles[i], argsHandler);

                        cgfData.Add(data);
                    }

                    #endregion

                    #region Render Output Files

                    if (argsHandler.Blend == true)
                    {
                        Blender blendFile = new Blender();
                        blendFile.WriteBlend(cgfData.Last());
                    }

                    if (argsHandler.Obj == true)
                    {
                        ObjFile objFile = new ObjFile();

                        foreach (var data in cgfData)
                            objFile.WriteObjFile(data); // cgfData.Last());
                    }

                    if (argsHandler.COLLADA == true)
                    {
                        COLLADA daeFile = new COLLADA();
                        daeFile.WriteCollada(cgfData.Last());
                    }

                    #endregion
                }
            }
            catch (Exception)
            {
                if (argsHandler.Throw)
                    throw;
            }

            return result;
        }
    }
}
