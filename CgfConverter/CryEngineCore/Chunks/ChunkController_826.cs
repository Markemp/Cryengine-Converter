using Extensions;
using System;
using System.IO;

namespace CgfConverter.CryEngineCore
{
    public class ChunkController_826 : ChunkController
    {
        public override void Read(BinaryReader b)
        {
            base.Read(b);

            //Utils.Log(LogLevelEnum.Debug, "ID is:  {0}", id);
            ControllerType = (CtrlType)Enum.ToObject(typeof(CtrlType), b.ReadUInt32());
            NumKeys = b.ReadUInt32();
            ControllerFlags = b.ReadUInt32();
            ControllerID = b.ReadUInt32();
            Keys = new Key[NumKeys];

            for (int i = 0; i < NumKeys; i++)
            {
                // Will implement fully later.  Not sure I understand the structure, or if it's necessary.
                Keys[i].Time = b.ReadInt32();
                // Utils.Log(LogLevelEnum.Debug, "Time {0}", Keys[i].Time);
                Keys[i].AbsPos = b.ReadVector3();
                Keys[i].RelPos = b.ReadVector3();
            }
        }
    }
}