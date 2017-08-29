using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngine_Core
{
    public abstract class ChunkCompiledBones : Chunk     //  0xACDC0000:  Bones info
    {
        public String RootBoneName;         // Controller ID?  Name?  Not sure yet.
        public CompiledBone RootBone;       // First bone in the data structure.  Usually Bip01
        public int NumBones;                // Number of bones in the chunk
        
        // Bone info
        // Bones are a bit different than Node Chunks, since there is only one CompiledBones Chunk, and it contains all the bones in the model.
        public Dictionary<String, CompiledBone> BoneDictionary = new Dictionary<String, CompiledBone>();  // Dictionary of all the CompiledBone objects based on bone name.
        public List<CompiledBone> BoneList = new List<CompiledBone>();

        public CompiledBone GetParentBone(CompiledBone bone)
        {
            if (bone.parentID != 0)
            {
                return BoneList.Where(a => a.parentID == bone.parentID).FirstOrDefault();
            }
            else 
                return bone;                // No parent bone found, so just returning itself.  CompiledBone is non-nullable.
        }

        public List<CompiledBone> GetAllChildBones(CompiledBone bone)
        {
            if (bone.numChildren > 0)
            {
                return BoneList.Where(a => bone.childIDs.Contains(a.controllerID)).ToList();
            }
            else
                return null;
        }

        protected void AddChildIDToParent(CompiledBone bone)
        {
            // Root bone parent ID will be zero.
            if (bone.parentID != 0)
            {
                CompiledBone parent = BoneList.Where(a => a.controllerID == bone.parentID).FirstOrDefault();  // Should only be one parent.
                parent.childIDs.Add(bone.controllerID);
            }
        }

        //public Vector3 TransformSoFar
        //{
        //    get
        //    {
        //        if (this.BoneList != null)
        //        {
        //            return this.ParentNode.TransformSoFar.Add(this.Transform.GetTranslation());
        //        }
        //        else
        //        {
        //            // TODO: What should this be?
        //            // return this._model.RootNode.Transform.GetTranslation();
        //            return this.Transform.GetTranslation();
        //        }
        //    }
        //}


        public override void WriteChunk()
        {
            Utils.Log(LogLevelEnum.Debug, "*** START CompiledBone Chunk ***");
            Utils.Log(LogLevelEnum.Debug, "    ChunkType:           {0}", ChunkType);
            Utils.Log(LogLevelEnum.Debug, "    Node ID:             {0:X}", ID);
        }
    }

}
