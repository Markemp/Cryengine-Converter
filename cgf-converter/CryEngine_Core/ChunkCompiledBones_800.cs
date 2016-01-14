using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public class ChunkCompiledBones_800 : ChunkCompiledBones
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            this.SkipBytes(b, 8);

            //  Read the first bone with ReadCompiledBone, then recursively grab all the children for each bone you find.
            //  Each bone structure is 584 bytes, so will need to seek childOffset * 584 each time, and go back.

            this.GetCompiledBones(b, "isRoot");                        // Start reading at the root bone
        }

        public void GetCompiledBones(BinaryReader b, String parent)        // Recursive call to read the bone at the current seek, and all children.
        {
            // Start reading all the properties of this bone.
            CompiledBone tempBone = new CompiledBone();
            // Utils.Log(LogLevelEnum.Debug, "** Current offset {0:X}", b.BaseStream.Position);
            tempBone.offset = b.BaseStream.Position;
            tempBone.ReadCompiledBone(b);
            tempBone.parentID = parent;
            //tempBone.WriteCompiledBone();
            tempBone.childNames = new String[tempBone.numChildren];
            this.BoneDictionary[tempBone.boneName] = tempBone;         // Add this bone to the dictionary.

            for (Int32 i = 0; i < tempBone.numChildren; i++)
            {
                // If child offset is 1, then we're at the right position anyway.  If it's 2, you want to 584 bytes.  3 is (584*2)...
                // Move to the offset of child.  If there are no children, we shouldn't move at all.
                b.BaseStream.Seek(tempBone.offset + 584 * tempBone.offsetChild + (i * 584), 0);
                this.GetCompiledBones(b, tempBone.boneName);
            }
            // Need to set the seek position back to the parent at this point?  Can use parent offset * 584...  Parent offset is a neg number
            //Utils.Log(LogLevelEnum.Debug, "Parent offset: {0}", tempBone.offsetParent);
        }

    }
}
