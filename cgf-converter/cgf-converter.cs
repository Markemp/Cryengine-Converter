using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Math;

namespace CgfConverter
{
    public class CgfData // Stores all information about the cgf file format.
    {
        // public string CgfFile; //the name of the file we are reading.  Removed to use dataFile, a File object
        // Header, ChunkTable and Chunks are what are in a file.  1 header, 1 table, and a chunk for each entry in the table.
        public FileInfo DataFile;
        public FileInfo objOutputFile;  // want this to be the datafile minus the extension and .obj
        public FileInfo MtlFile;        // the Material file
        public Header CgfHeader;
        public ChunkTable CgfChunkTable;
        public List<Chunk> CgfChunks = new List<Chunk>();   //  I don't think we want this.  Dictionary is better because of ID
        // ChunkDictionary will let us get the Chunk from the ID.
        public Dictionary<UInt32, Chunk> ChunkDictionary = new Dictionary<UInt32, Chunk>();
        private UInt32 RootNodeID;
        private ChunkNode RootNode;


        public void ReadCgfData(FileInfo dataFile)  // Constructor for CgfFormat.  This populates the structure
        {
            // CgfFile = cgffile;  // Don't use string, use File Obj
            //using (BinaryReader cgfreader = new BinaryReader(File.Open(dataFile.ToString(), FileMode.Open)))
            DataFile = dataFile;
            BinaryReader cgfreader = new BinaryReader(File.Open(dataFile.ToString(), FileMode.Open));
            CgfHeader = new Header(cgfreader);// Gets the header of the file (3-5 objects dep on version)
            // CgfHeader.WriteChunk();
            int offset = CgfHeader.fileOffset;  // location of the Chunk table.
            cgfreader.BaseStream.Seek(offset, 0);  // will now start to read from the start of the chunk table
            CgfChunkTable = new ChunkTable();
            CgfChunkTable.GetChunkTable(cgfreader, offset);

            foreach (ChunkHeader ChkHdr in CgfChunkTable.chunkHeaders) 
            {
                ChunkType chkType = ChkHdr.type;
                //Console.WriteLine("Processing {0}", chkType);
                switch (ChkHdr.type)
                {
                    case ChunkType.SourceInfo:
                    {
                        ChunkSourceInfo chkSrcInfo = new ChunkSourceInfo();
                        chkSrcInfo.ReadChunk(cgfreader,ChkHdr.offset);
                        CgfChunks.Add(chkSrcInfo);
                        ChunkDictionary.Add(chkSrcInfo.id, chkSrcInfo);
                        // chkSrcInfo.WriteChunk(); 
                        break;
                    }
                    case ChunkType.Timing:
                    {
                        ChunkTimingFormat chkTiming = new ChunkTimingFormat();
                        chkTiming.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkTiming);
                        ChunkDictionary.Add(chkTiming.id, chkTiming);
                        //chkTiming.WriteChunk();
                        break;
                    }
                    case ChunkType.ExportFlags:
                    {
                        ChunkExportFlags chkExportFlag = new ChunkExportFlags();
                        chkExportFlag.ReadChunk(cgfreader, ChkHdr.offset);
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
                        chkMtlName.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMtlName);
                        ChunkDictionary.Add(chkMtlName.id, chkMtlName);
                        chkMtlName.WriteChunk();
                        break;
                    }
                    case ChunkType.DataStream:
                    {
                        ChunkDataStream chkDataStream = new ChunkDataStream();
                        chkDataStream.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkDataStream);
                        ChunkDictionary.Add(chkDataStream.id, chkDataStream);
                        //chkDataStream.WriteChunk();
                        break;
                    }
                         
                    case ChunkType.Mesh:
                    {
                        ChunkMesh chkMesh = new ChunkMesh();
                        chkMesh.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMesh);
                        ChunkDictionary.Add(chkMesh.id, chkMesh);
                        //chkMesh.WriteChunk();
                        break;
                    }
                    case ChunkType.MeshSubsets:
                    {
                        ChunkMeshSubsets chkMeshSubsets = new ChunkMeshSubsets();
                        chkMeshSubsets.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkMeshSubsets);
                        ChunkDictionary.Add(chkMeshSubsets.id, chkMeshSubsets);
                        chkMeshSubsets.WriteChunk();
                        break;
                    }
                    case ChunkType.Node:
                    {
                        ChunkNode chkNode = new ChunkNode();
                        chkNode.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkNode);
                        ChunkDictionary.Add(chkNode.id, chkNode);
                        if (chkNode.Parent == 0xFFFFFFFF)
                        {
                            Console.WriteLine("Found a Parent chunk node.  Adding to the dictionary.");
                            RootNodeID = chkNode.id;
                            RootNode = chkNode;
                            // ChunkDictionary[RootNodeID].WriteChunk();
                        }
                        //chkNode.WriteChunk();
                        break;
                    }
                    case ChunkType.Helper:
                    {
                        ChunkHelper chkHelper = new ChunkHelper();
                        chkHelper.ReadChunk(cgfreader, ChkHdr.offset);
                        CgfChunks.Add(chkHelper);
                        //chkHelper.WriteChunk();
                        break;
                    }
                    default:
                    {
                        //Console.WriteLine("Chunk type found that didn't match known versions: {0}",ChkHdr.type);
                        break;
                    }
                }
            }
            // Test:  Let's write the parent node chunk.  ID is 6B4 for SHornet
            ChunkDictionary[RootNodeID].WriteChunk();
        }

        public void WriteObjFile()
        {
            // At this point, we should have a CgfData object, fully populated.
            // We need to create the obj header, then for each submech write the vertex, UV and normal data.
            // First, let's figure out the name of the output file.  Should be <object name>.obj
            // Console.WriteLine("Output file is {0}", objOutputFile.Name);

            // Each Mesh will have a mesh subset and a series of datastream objects.  Need temporary pointers to these
            // so we can manipulate
            Console.WriteLine();
            Console.WriteLine("*** Starting WriteObjFile() ***");
            Console.WriteLine();

            // Get object name.  This is the Root Node chunk Name
            // Get the objOutputFile name
            objOutputFile = new FileInfo(RootNode.Name + ".obj");
            Console.WriteLine("Output File name is {0}", objOutputFile.Name);

            using  (System.IO.StreamWriter file = new System.IO.StreamWriter(objOutputFile.Name))
            {
                string s1 = String.Format("# cgf-converter .obj export Version 0.1");
                file.WriteLine(s1);
                file.WriteLine("#");
                
                // Console.WriteLine(RootNode.NumChildren);
                if (RootNode.NumChildren == 0)
                {
                    // write out the material library
                    ChunkMtlName tmpMtlName = (ChunkMtlName)ChunkDictionary[RootNode.MatID];
                    string s2 = String.Format("mtllib {0}", tmpMtlName.Name + ".mtl");
                    file.WriteLine(s2);
                    // We have a root node with no children, so simple object.
                    string s3 = String.Format("o {0}", RootNode.Name);
                    file.WriteLine(s3);

                    // Now grab the mesh and process that.  RootNode[ObjID] is the Mesh for this Node.
                    //ChunkMesh tmpMesh = (ChunkMesh)ChunkDictionary[RootNode.Object];
                    //tmpMesh.WriteChunk();
                    //ChunkMeshSubsets tmpMeshSubSet = (ChunkMeshSubsets)ChunkDictionary[tmpMesh.MeshSubsets];
                    this.WriteObjNode(file, RootNode);

                }
                else
                {
                    // Not a simple object.  Will need to recursively call WriteObjNode
                    // Write out the mtl lib.  For MatType of 0x10, only one mtl in town.
                    foreach (ChunkMtlName tmpMtlName in CgfChunks.Where(a => a.chunkType == ChunkType.MtlName))
                    {
                        if (tmpMtlName.MatType == 0x01)
                        {
                            Console.WriteLine("Found the material file...");
                            string s_material = String.Format("mtllib {0}", tmpMtlName.Name + ".mtl");
                            file.WriteLine(s_material);
                        }
                    }
                    foreach (ChunkNode tmpNode in CgfChunks.Where(a => a.chunkType == ChunkType.Node))
                    {
                        if (tmpNode.MatID != 0)  // because we don't want to process an object with no material.
                        {
                            tmpNode.WriteChunk();
                            //ChunkMtlName tmpMtlName = (ChunkMtlName)ChunkDictionary[tmpNode.MatID];
                            //string s2 = String.Format("mtllib {0}", tmpMtlName.Name + ".mtl");
                            //file.WriteLine(s2);
                            string s3 = String.Format("o {0}", tmpNode.Name);
                            file.WriteLine(s3);
                            // Grab the mesh and process that.
                            this.WriteObjNode(file, tmpNode);
                        }
                    }
                }
            }  // End of writing the output file
        }
        // Structures

        public void WriteObjNode(StreamWriter f, ChunkNode chunkNode)  // Pass a node to this to have it write to the Stream
        {
            Console.WriteLine("*** Processing Chunk node {0:X}", chunkNode.id);
            Console.WriteLine("***     Object ID {0:X}", chunkNode.Object);
            ChunkDictionary[chunkNode.Object].WriteChunk();
            ChunkMesh tmpMesh = (ChunkMesh)ChunkDictionary[chunkNode.Object];
            if (tmpMesh.MeshSubsets == 0)
            {
                Console.WriteLine("Found a node chunk with no mesh subsets.  Proxy.  Skipping...");
                return;
            }
            ChunkMtlName tmpMtlName = (ChunkMtlName)ChunkDictionary[chunkNode.MatID];
            ChunkMeshSubsets tmpMeshSubsets = (ChunkMeshSubsets)ChunkDictionary[tmpMesh.MeshSubsets];
            ChunkDataStream tmpVertices = (ChunkDataStream)ChunkDictionary[tmpMesh.VerticesData];
            ChunkDataStream tmpNormals = (ChunkDataStream)ChunkDictionary[tmpMesh.NormalsData];
            ChunkDataStream tmpUVs = (ChunkDataStream)ChunkDictionary[tmpMesh.UVsData];
            ChunkDataStream tmpIndices = (ChunkDataStream)ChunkDictionary[tmpMesh.IndicesData];
            uint numChildren = chunkNode.NumChildren;           // use in a for loop to print the mesh for each child

            if (numChildren != 0)
            {
                // Need a way to get all the children, so they can each be passed recursively
            }
            Console.WriteLine("In WriteObjNode");
            //chunkNode.WriteChunk();
            //tmpMesh.WriteChunk();
            //tmpMeshSubsets.WriteChunk();
            //tmpMtlName.WriteChunk();
            for (int i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
            {
                // Write vertices data for each MeshSubSet
                Console.WriteLine("Write out datastreams");
                f.WriteLine("g");
                for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j++)
                {
                    string s4 = String.Format("v {0:F7} {1:F7} {2:F7}", tmpVertices.Vertices[j].x, tmpVertices.Vertices[j].y, tmpVertices.Vertices[j].z);
                    f.WriteLine(s4);
                }
                f.WriteLine();
                for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j++)
                {
                    string s5 = String.Format("vt {0:F7} {1:F7} 0.0 ", tmpUVs.UVs[j].U, 1 - tmpUVs.UVs[j].V);
                    f.WriteLine(s5);
                }
                f.WriteLine();
                for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j < (int)tmpMeshSubsets.MeshSubsets[i].NumVertices + (int)tmpMeshSubsets.MeshSubsets[i].FirstVertex; j++)
                {
                    string s6 = String.Format("vn {0:F7} {1:F7} {2:F7}", tmpNormals.Normals[j].x, tmpNormals.Normals[j].y, tmpNormals.Normals[j].z);
                    f.WriteLine(s6);
                }
                f.WriteLine();
                string s7 = String.Format("g {0}", chunkNode.Name);
                f.WriteLine(s7);
                string s_material = String.Format("usemtl {0}", tmpMeshSubsets.MeshSubsets[i].MatID);
                f.WriteLine(s_material);
                /*if (tmpMtlName.MatType == 0x10)
                {
                    // Only one material file.  Material is an index to the .mtl file.  If the submesh material ID is 2, it's the 2nd item in the mtl file
                    string s_material = String.Format("usemtl {0}", tmpMeshSubsets.MeshSubsets[i].MatID);
                    f.WriteLine(s_material);
                }
                else if (tmpMtlName.MatType == 0x01 || tmpMtlName.MatType == 0x0)  // The material ID assigned to this node is a parent, so there are children for each submesh.
                {
                    //ChunkMtlName tmpSubMtlName = (ChunkMtlName)ChunkDictionary[tmpMtlName.Children[i]];
                    //Console.WriteLine("Submesh Material");
                    //tmpSubMtlName.WriteChunk();
                    string s8 = String.Format("usemtl {0}", tmpMeshSubsets.MeshSubsets[i].MatID);
                    f.WriteLine(s8);
                }*/
                // Now write out the faces info based on the MtlName
                for (int j = (int)tmpMeshSubsets.MeshSubsets[i].FirstIndex; j < (int)tmpMeshSubsets.MeshSubsets[i].NumIndices + (int)tmpMeshSubsets.MeshSubsets[i].FirstIndex; j++)
                {
                    string s9 = String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", tmpIndices.Indices[j] + 1, tmpIndices.Indices[j + 1] + 1, tmpIndices.Indices[j + 2] + 1);
                    f.WriteLine(s9);
                    j = j + 2;
                }
                f.WriteLine();
            }
        }
        
        public class Header
        {
            public char[] fileSignature; // The CGF file signature.  CryTek for 3.5, CrChF for 3.6
            public uint fileType; // The CGF file type (geometry or animation)  3.5 only
            public uint chunkVersion; // The version of the chunk table  3.5 only
            public int fileOffset; //Position of the chunk table in the CGF file
            public int numChunks; // Number of chunks in the Chunk Table (3.6 only.  3.5 has it in Chunk Table)

            // methods
            public Header(BinaryReader binReader)  //constructor with 1 arg
            {
                //Header cgfHeader = new Header();
                // populate the Header objects
                fileSignature = new char[8];
                fileSignature = binReader.ReadChars(8);
                fileType = binReader.ReadUInt32();
                chunkVersion = binReader.ReadUInt32();
                fileOffset = binReader.ReadInt32();

                return;
            }
            public void WriteChunk()  // output header to console for testing
            {
                string tmpFileSig;
                tmpFileSig = new string(fileSignature);
                Console.WriteLine("*** HEADER ***");
                Console.WriteLine("    Header Filesignature: {0}", tmpFileSig);
                Console.WriteLine("    Header FileType: {0:X}", fileType);
                Console.WriteLine("    Header ChunkVersion: {0:X}", chunkVersion);
                Console.WriteLine("    Header ChunkTableOffset: {0:X}", fileOffset);
                Console.WriteLine("*** END HEADER ***");
                return;
            }
        }
        public class ChunkTable  // reads the chunk table
        {
            public uint numChunks;
            public List<ChunkHeader> chunkHeaders = new List<ChunkHeader>();

            // methods
            public void GetChunkTable(BinaryReader b, int f)
            {
                // need to seek to the start of the table here.  foffset points to the start of the table
                b.BaseStream.Seek(f, 0);
                numChunks = b.ReadUInt32();  // number of Chunks in the table.
                int i; // counter for loop to read all the chunkHeaders
                for (i = 0; i < numChunks; i++)
                {
                    //Console.WriteLine("Loop {0}", i);
                    ChunkHeader tempChkHdr = new ChunkHeader(); // Add this chunk header to the list
                    uint headerType = b.ReadUInt32(); // read the value, then parse it
                    tempChkHdr.type = (ChunkType)Enum.ToObject(typeof(ChunkType), headerType);
                    //Console.WriteLine("headerType: '{0}'", tempChkHdr.type);
                    uint chunkversionType = b.ReadUInt32();
                    tempChkHdr.version = (ChunkVersion)Enum.ToObject(typeof(ChunkVersion), chunkversionType);
                    tempChkHdr.offset = b.ReadUInt32();
                    tempChkHdr.id = b.ReadUInt32();  // This is the reference number to identify the mesh/datastream
                    tempChkHdr.unknown = b.ReadUInt32();

                    chunkHeaders.Add(tempChkHdr);
                    //tempChkHdr.WriteChunk();
                }
            }
            public void WriteChunk()
            {
                foreach (ChunkHeader chkHdr in chunkHeaders)
                {
                    chkHdr.WriteChunk();
                }
            }
        }
        public class ChunkHeader
        {
            public ChunkType type;
            public ChunkVersion version;
            public uint offset;
            public uint id;
            public uint unknown; //  TBD

            // methods
            /*public void GetChunkHeader()
            {
                type = new ChunkType();
                version = new ChunkVersion();
                offset = new uint();
                id = new uint();
                unknown = new uint();
            }*/
            // never used; refactor
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
 // Main class has Fileoffset to identify where the chunk starts
        {
            public FileOffset fOffset = new FileOffset();  // starting offset of the chunk.  Int, not uint
            public ChunkType chunkType; // Type of chunk
            public uint version; // version of this chunk
            public uint id; // ID of the chunk.
            // public int offset; // simpler way of getting fOffset;  Will need to refactor eventually
            // These will be uninstantiated if it's not that kind of chunk
            //public ChunkTimingFormat chunkTimingFormat;
            //public ChunkMtlName chunkMtlName;
            //public ChunkExportFlags chunkExportFlags;
            //public ChunkSourceInfo chunkSourceInfo;
            //public ChunkDataStream chunkDataStream;
            //public ChunkMeshSubsets chunkMeshSubsets;
            //public ChunkMesh chunkMesh;
            //public ChunkNode chunkNode;
            //public ChunkHelper chunkHelper;

            public virtual void ReadChunk(BinaryReader b, uint f)
            {
                // Don't do anything.  This is just a placeholder
            }
            public virtual void WriteChunk()
            {
                // Don't do anything.  Placeholder
            }
        }
        public class ChunkHelper : Chunk       // cccc0001:  Helper chunk.  This is the top level, then nodes, then mesh, then mesh subsets
        {
            public string Name;
            public HelperType Type;
            public Vector3 Pos;
            public Matrix44 Transform;

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Timing Format chunk
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), b.ReadUInt32());
                version = b.ReadUInt32();
                fOffset.Offset = b.ReadInt32();
                id = b.ReadUInt32();
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
                Console.WriteLine("*** END Helper Chunk ***");
            }
        }
        public class ChunkNode : Chunk          // cccc000b:   Node
        {
            public string Name;  // String 64.
            public uint Object;  // Mesh or Helper Object chunk ID
            public uint Parent;  // Node parent.  if 0xFFFFFFFF, it's the top node.
            public uint NumChildren;
            public uint MatID;  // reference to the material ID for this Node chunk
            public Boolean IsGroupHead; //
            public Boolean IsGroupMember;
            public byte[] Reserved1; // padding, 2 bytes long
            public Matrix44 Transform;   // Transformation matrix
            public Vector3 Pos;  // position vector of above transform
            public Quat Rot;     // rotation component of above transform
            public Vector3 Scl;  // Scalar component of above matrix44
            public uint PosCtrl;  // Position Controller ID (Controller Chunk type)
            public uint RotCtrl;  // Rotation Controller ID 
            public uint SclCtrl;  // Scalar controller ID
            // These are children, materials, etc.
            public ChunkMtlName MaterialChunk;
            public ChunkNode[] NodeChildren;


            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the Node chunk
                uint tmpNodeChunk = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpNodeChunk);
                version = b.ReadUInt32();
                fOffset.Offset = b.ReadInt32();
                id = b.ReadUInt32();
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
                NumChildren = b.ReadUInt32();
                MatID = b.ReadUInt32();  // Material ID?
                // chunkNode = this;
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
                Console.WriteLine("*** END Node Chunk ***");

            }
        }
        public class ChunkTimingFormat : Chunk // cccc000e:  Timing format chunk
        {
            // public ChunkType ChunkTiming;    // Moved to Chunk
            // public uint Version;             // Moved to Chunk
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
                version = b.ReadUInt32();  //0x00000918 is Far Cry, Crysis, MWO, Aion
                //fOffset = b.ReadUInt32();
                //id = b.ReadUInt32();
                SecsPerTick = b.ReadSingle();
                TicksPerFrame = b.ReadInt32();
                Unknown1 = b.ReadUInt32();
                Unknown2 = b.ReadUInt32();
                GlobalRange.Name = new char[32];
                GlobalRange.Name = b.ReadChars(32);  // Name is technically a String32, but F those structs
                GlobalRange.Start = b.ReadInt32();
                GlobalRange.End = b.ReadInt32();
                //chunkTimingFormat = this;
            }
            public override void WriteChunk()
            {
                string tmpName = new string(GlobalRange.Name);
                Console.WriteLine("*** TIMING CHUNK ***");
                Console.WriteLine("    Version: {0:X}", version);
                Console.WriteLine("    Secs Per Tick: {0}", SecsPerTick);
                Console.WriteLine("    Ticks Per Frame: {0}", TicksPerFrame);
                Console.WriteLine("    Global Range:  Name: {0}", tmpName);
                Console.WriteLine("    Global Range:  Start: {0}", GlobalRange.Start);
                Console.WriteLine("    Global Range:  End:  {0}", GlobalRange.End);
                Console.WriteLine("*** END TIMING CHUNK ***");
            }
        }
        public class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
        {
            // public ChunkType ExportFlag;
            //public uint Version;  // Far Cry and Crysis:  1
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

            //<version num="1">Far Cry, Crysis</version>
            //<add name="Flags" type="ExportFlags" />
            //<add name="RC Version" type="uint" arr1="4" />
            //<add name="RC Version String" type="String16" />
            //<add name="Reserved" type="uint" arr1="32" />


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
                id = 0xFF;
                // chunkSourceInfo = this;
            }
            public override void WriteChunk()
            {
                Console.WriteLine("*** SOURCE INFO CHUNK ***");
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
            public uint NumChildren802; // for type 802, unknown value (number of materials for type 802?)
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
                uint tmpChunkMtlName = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkMtlName);
                version = b.ReadUInt32();
                fOffset.Offset = b.ReadInt32();  // offset to this chunk
                id = b.ReadUInt32();  // ref/chunk number
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
                    NumChildren802 = b.ReadUInt32();  // number of materials
                    PhysicsTypeArray = new MtlNamePhysicsType[NumChildren802];
                    for (int i = 0; i < NumChildren802; i++)
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
            public ushort[] Indices;    // for dataStreamType of 5, length is NumElements.
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
                uint tmpChunkDataStream = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkDataStream);
                version = b.ReadUInt32();
                Flags = b.ReadUInt32();  // Offset to this chunk
                //int tmpfOffset = b.ReadInt32();
                //fOffset = (FileOffset)Enum.ToObject(typeof(FileOffset), tmpfOffset);  // offset of this datastream chunk
                id = b.ReadUInt32();  // Reference to the data stream type!  4th word after the start of the chunk
                Flags2 = b.ReadUInt32(); // another filler
                uint tmpdataStreamType = b.ReadUInt32();
                dataStreamType = (DataStreamType)Enum.ToObject(typeof(DataStreamType), tmpdataStreamType);
                NumElements = b.ReadUInt32(); // number of elements in this chunk
                BytesPerElement = b.ReadUInt32(); // bytes per element
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
                                    // Console.WriteLine("{0} {1} {2} {3}", i, Vertices[i].x, Vertices[i].y, Vertices[i].z);
                                }

                            }
                            // Console.WriteLine("{0} elements read", VertexList.Length);
                            // Console.WriteLine("Offset is {0:X}", b.BaseStream.Position);
                            break;
                        }
                    case DataStreamType.INDICES:  // Ref is 
                        {
                            Indices = new ushort[NumElements];
                            for (int i = 0; i < NumElements; i++)
                            {
                                Indices[i] = b.ReadUInt16();
                                // Console.WriteLine("Indice {0} is {1}", i, Indices[i]);
                                if (i > NumElements - 20)
                                {
                                    //Console.WriteLine("Indices {0} is {1}", i, Indices[i]);
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
                    Console.WriteLine("           Number of Vertices:   {0}", MeshSubsets[i].NumVertices);
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
            public uint NumMeshSubsets; // 800, Number of Mesh subsets
            public uint MeshSubsets; // 800  Reference of the mesh subsets
            // public ChunkVertAnim VertAnims; // 744.  not implemented
            //public Vertex[] Vertices; // 744.  not implemented
            //public Face[,] Faces; // 744.  Not implemented
            //public UV[] UVs; // 744 Not implemented
            //public UVFace[] UVFaces; // 744 not implemented
            // public VertexWeight[] VertexWeights; // 744 not implemented
            //public IRGB[] VertexColors; // 744 not implemented
            public uint UnknownData; // 0x00 word here for some reason.
            public uint VerticesData; // 800
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
            public uint[] ReservedData = new uint[4]; // 800 Length 4
            public uint[] PhysicsData = new uint[4]; // 800
            public Vector3 MinBound; // 800 minimum coordinate values
            public Vector3 MaxBound; // 800 Max coord values
            public uint[] Reserved3 = new uint[32]; // 800 array of 32 uint values.

            //public ChunkMeshSubsets chunkMeshSubset; // pointer to the mesh subset that belongs to this mesh

            public override void ReadChunk(BinaryReader b, uint f)
            {
                b.BaseStream.Seek(f, 0); // seek to the beginning of the MeshSubset chunk
                uint tmpChunkType = b.ReadUInt32();
                chunkType = (ChunkType)Enum.ToObject(typeof(ChunkType), tmpChunkType);
                version = b.ReadUInt32();
                Flags1 = b.ReadUInt32();  // offset
                id = b.ReadUInt32();  // Chunk ID  0x23 for candle
                Unknown1 = b.ReadUInt32();  // unknown
                Unknown1 = b.ReadUInt32();  // unknown
                NumVertices = b.ReadUInt32();
                NumIndices = b.ReadUInt32();   //
                NumMeshSubsets = b.ReadUInt32();
                MeshSubsets = b.ReadUInt32();  // refers to ID in mesh subsets  1d for candle
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
                MinBound.x = b.ReadUInt32();
                MinBound.y = b.ReadUInt32();
                MinBound.z = b.ReadUInt32();
                MaxBound.x = b.ReadUInt32();
                //chunkMesh = this;
                // Not going to read the Reserved 32 element array.
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
                Console.WriteLine("*** END MESH CHUNK ***");
            }
        }

        public class FileSignature          // the signature that Cryengine files start with.  Crytek or CrChF 
        {
            public String Read(BinaryReader b)  // Checks the signature
            {
                char[] signature = new char[8];  // first 8 bytes are the file signature.
                signature = b.ReadChars(8);
                string s = new string(signature);
                return s;
            }
        }

    }


    // Aliases
    public class FileOffset
    { 
        public int Offset;
    }

    // classes (aka anything more complicated than a fixed size struct, with methods etc.)


    public class ArgsHandler
    {
        // all the possible switches
        public Boolean Usage;
        public Boolean FlipUVs;
        public FileInfo InputFile;
        public FileInfo OutputFile;
        public Boolean Obj;
        public Boolean Blend;


    }


    class Program
    {
        /*static void ReadCryHeader(string cgfFile)
        {
            //TBD
        }
        static void ReadChunkTable(string cgfFile)
        {
            // TBD
        }*/  // These 2 functions not needed.  To be implemented in classes

        static void Main(string[] args)
        {
            // Assign the argument to a variable
            FileInfo dataFile;  // This is the file passed in args

            if (args.Length != 0 && File.Exists(args[0]))
            {
                dataFile = new FileInfo(args[0]);  // preferred.  We want objects, not strings
            }
            else
            {
                Console.WriteLine("Please input cgf/cga/chrparams file.");
                dataFile = new FileInfo(Console.ReadLine());  // This needs error checking.
            }
            
            // Console.WriteLine("Input File is '{0}'" , dataFile.Name);
            // ReadCryHeader(cgfFile);
            CgfData cgfData = new CgfData();
            cgfData.ReadCgfData(dataFile);
            
            // Output to an obj file
            cgfData.WriteObjFile();  

            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey(); // Press any key to continue
            return;
        }
    }
}
