﻿using System;
using System.IO;

namespace CgfConverter.CryEngineCore;

public class ChunkSourceInfo_0 : ChunkSourceInfo
{
    public override void Read(BinaryReader reader)
    {
        this.ChunkType = this._header.ChunkType;
        this.Version = this._header.Version;
        this.Offset = this._header.Offset;
        this.ID = this._header.ID;
        this.Size = this._header.Size;

        reader.BaseStream.Seek(this._header.Offset, SeekOrigin.Begin);

        uint peek = reader.ReadUInt32();

        // Try and detect SourceInfo type - if it's there, we need to skip ahead a few bytes
        if ((peek == (uint)ChunkType.SourceInfo) || (peek + 0xCCCBF000 == (uint)ChunkType.SourceInfo))
        {
            this.SkipBytes(reader, 12);
        }
        else
        {
            reader.BaseStream.Seek(this._header.Offset, 0);
        }

        if (this.Offset != this._header.Offset || this.Size != this._header.Size)
        {
            Utils.Log(LogLevelEnum.Warning, "Conflict in chunk definition:  SourceInfo chunk");
            Utils.Log(LogLevelEnum.Warning, "{0:X}+{1:X}", this._header.Offset, this._header.Size);
            Utils.Log(LogLevelEnum.Warning, "{0:X}+{1:X}", this.Offset, this.Size);
        }

        this.ChunkType = ChunkType.SourceInfo; // this chunk doesn't actually have the chunktype header.
        this.SourceFile = reader.ReadCString();
        this.Date = reader.ReadCString().TrimEnd(); // Strip off last 2 Characters, because it contains a return
        // It is possible that Date has a newline in it instead of a null.  If so, split it based on newline.  Otherwise read Author.
        if (this.Date.Contains('\n'))
        {
            this.Author = this.Date.Split('\n')[1];
            this.Date = this.Date.Split('\n')[0];
        }
        else
        {
            this.Author = reader.ReadCString();
        }
    }
}
