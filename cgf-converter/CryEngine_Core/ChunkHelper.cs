using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    /// <summary>
    /// Helper chunk.  This is the top level, then nodes, then mesh, then mesh subsets
    /// CCCC0001  
    /// </summary>
    public abstract class ChunkHelper : CryEngine_Core.Chunk
    {
        public String Name;
        public HelperTypeEnum HelperType;
        public Vector3 Pos;
        public Matrix44 Transform;

        public override void WriteChunk()
        {
            Console.WriteLine("*** START Helper Chunk ***");
            Console.WriteLine("    ChunkType:   {0}", ChunkType);
            Console.WriteLine("    Version:     {0:X}", Version);
            Console.WriteLine("    ID:          {0:X}", ID);
            Console.WriteLine("    HelperType:  {0}", HelperType);
            Console.WriteLine("    Position:    {0}, {1}, {2}", Pos.x, Pos.y, Pos.z);
            Console.WriteLine("*** END Helper Chunk ***");
        }
    }
}
