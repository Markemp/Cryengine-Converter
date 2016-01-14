using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkMtlName : CryEngine_Core.Chunk  // cccc0014:  provides material name as used in the .mtl file
    {
        // need to find the material ID used by the mesh subsets
        public UInt32 Flags1 { get; internal set; }  // pointer to the start of this chunk?
        public UInt32 MatType { get; internal set; } // for type 800, 0x1 is material library, 0x12 is child, 0x10 is solo material
        //public UInt32 NumChildren802; // for type 802, NumChildren
        public String Name { get; internal set; } // technically a String128 class
        public MtlNamePhysicsType PhysicsType { get; internal set; } // enum of a 4 byte UInt32  For 802 it's an array, 800 a single element.
        public MtlNamePhysicsType[] PhysicsTypeArray { get; internal set; } // enum of a 4 byte UInt32  For 802 it's an array, 800 a single element.
        public UInt32 NumChildren { get; internal set; } // number of materials in this name. Max is 66
        // need to implement an array of references here?  Name of Children
        public UInt32[] Children { get; internal set; }
        public UInt32 AdvancedData { get; internal set; }  // probably not used
        public Single Opacity { get; internal set; } // probably not used

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START MATERIAL NAMES ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:           {0}", this.ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Material Name:       {0}", this.Name);
            Utils.Log(LogLevelEnum.Verbose, "    Material ID:         {0:X}", this.ID);
            Utils.Log(LogLevelEnum.Verbose, "    Version:             {0:X}", this.Version);
            Utils.Log(LogLevelEnum.Verbose, "    Number of Children:  {0}", this.NumChildren);
            Utils.Log(LogLevelEnum.Verbose, "    Material Type:       {0:X}", this.MatType); // 0x1 is mtllib w children, 0x10 is mtl no children, 0x18 is child
            Utils.Log(LogLevelEnum.Verbose, "    Physics Type:        {0}", this.PhysicsType);
            Utils.Log(LogLevelEnum.Verbose, "*** END MATERIAL NAMES ***");
        }
    }
}
