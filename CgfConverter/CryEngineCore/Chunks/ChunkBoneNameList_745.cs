﻿using System;
using System.IO;
using System.Linq;

namespace CgfConverter.CryEngineCore;

internal sealed class ChunkBoneNameList_745 : ChunkBoneNameList
{
    public override void Read(BinaryReader b)
    {
        base.Read(b);
        BoneNames = b.ReadCString().Split(' ').ToList();
    }
}
