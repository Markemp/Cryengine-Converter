using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class STerrainChunkHeader
{
    public readonly byte Version;
    public readonly byte Dummy;
    public readonly byte Flags;
    public readonly byte Flags2;
    public readonly int ChunkSize;
    public readonly STerrainInfo TerrainInfo;

    public STerrainChunkHeader(BinaryReader reader)
    {
        reader.ReadInto(out Version);
        switch (Version)
        {
            case >= 24 and <= 29:
                reader.ReadInto(out Dummy);
                reader.ReadInto(out Flags);
                reader.ReadInto(out Flags2);
                reader.ReadInto(out ChunkSize);
                TerrainInfo = new STerrainInfo(reader, Version);
                return;
        }

        throw new NotSupportedException();
    }
}