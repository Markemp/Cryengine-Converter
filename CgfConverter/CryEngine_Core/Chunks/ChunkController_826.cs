using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.CryEngineCore
{
    public class ChunkController_826 : ChunkController
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            //Utils.Log(LogLevelEnum.Debug, "ID is:  {0}", id);
            this.ControllerType = (CtrlType)Enum.ToObject(typeof(CtrlType), b.ReadUInt32());
            this.NumKeys = b.ReadUInt32();
            this.ControllerFlags = b.ReadUInt32();
            this.ControllerID = b.ReadUInt32();
            this.Keys = new Key[NumKeys];

            for (Int32 i = 0; i < this.NumKeys; i++)
            {
                // Will implement fully later.  Not sure I understand the structure, or if it's necessary.
                this.Keys[i].Time = b.ReadInt32();
                // Utils.Log(LogLevelEnum.Debug, "Time {0}", Keys[i].Time);
                this.Keys[i].AbsPos.x = b.ReadSingle();
                this.Keys[i].AbsPos.y = b.ReadSingle();
                this.Keys[i].AbsPos.z = b.ReadSingle();
                // Utils.Log(LogLevelEnum.Debug, "Abs Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].AbsPos.x, Keys[i].AbsPos.y, Keys[i].AbsPos.z);
                this.Keys[i].RelPos.x = b.ReadSingle();
                this.Keys[i].RelPos.y = b.ReadSingle();
                this.Keys[i].RelPos.z = b.ReadSingle();
                // Utils.Log(LogLevelEnum.Debug, "Rel Pos: {0:F7}  {1:F7}  {2:F7}", Keys[i].RelPos.x, Keys[i].RelPos.y, Keys[i].RelPos.z);
            }
        }
    }
}