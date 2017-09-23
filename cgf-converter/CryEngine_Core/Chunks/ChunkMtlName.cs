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
        /// <summary>
        /// Type of Material associated with this name
        /// </summary>
        public MtlNameTypeEnum MatType { get; internal set; }
        /// <summary>
        /// Name of the Material
        /// </summary>
        public String Name { get; internal set; }
        public MtlNamePhysicsType[] PhysicsType { get; internal set; }
        /// <summary>
        /// Number of Materials in this name (Max: 66)
        /// </summary>
        public UInt32 NumChildren { get; internal set; } 
        public UInt32[] ChildIDs { get; internal set; }

        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Verbose, "*** START MATERIAL NAMES ***");
            Utils.Log(LogLevelEnum.Verbose, "    ChunkType:           {0} ({0:X})", this.ChunkType);
            Utils.Log(LogLevelEnum.Verbose, "    Material Name:       {0}", this.Name);
            Utils.Log(LogLevelEnum.Verbose, "    Material ID:         {0:X}", this.ID);
            Utils.Log(LogLevelEnum.Verbose, "    Version:             {0:X}", this.Version);
            Utils.Log(LogLevelEnum.Verbose, "    Number of Children:  {0}", this.NumChildren);
            Utils.Log(LogLevelEnum.Verbose, "    Material Type:       {0} ({0:X})", this.MatType);
            foreach (var physicsType in this.PhysicsType)
            {
                Utils.Log(LogLevelEnum.Verbose, "    Physics Type:        {0} ({0:X})", physicsType);
            }
            Utils.Log(LogLevelEnum.Verbose, "*** END MATERIAL NAMES ***");
        }
    }
}
