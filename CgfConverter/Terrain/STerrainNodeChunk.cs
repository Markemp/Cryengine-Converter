using System;
using System.IO;
using System.Linq;
using Extensions;

namespace CgfConverter.Terrain;

public class STerrainNodeChunk
{
    public readonly short ChunkVersion;
    public readonly short HasHoles;
    public readonly AaBb BoxHeightmap;
    public readonly float Offset;
    public readonly float Range;
    public readonly int Size;
    public readonly int SurfaceTypesNum;

    public readonly ushort[,] HeightMapData;
    public readonly float[] HeightMapErrors;
    public readonly byte[] SurfaceTypes;
    
    public readonly STerrainNodeChunk[] Children;

    public STerrainNodeChunk(BinaryReader reader, int unitsToSectorBitshift)
    {
        reader.ReadInto(out ChunkVersion);
        switch (ChunkVersion)
        {
            case 7:
                reader.ReadInto(out HasHoles);
                reader.ReadInto(out BoxHeightmap);
                reader.ReadInto(out Offset);
                reader.ReadInto(out Range);
                reader.ReadInto(out Size);
                reader.ReadInto(out SurfaceTypesNum);
                break;
            
            default:
                throw new NotSupportedException();
        }

        HeightMapData = new ushort[Size, Size];
        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Size; j++)
                reader.ReadInto(out HeightMapData[i, j]);
        }
        
        reader.AlignTo(4);
        HeightMapErrors = Enumerable.Range(0, unitsToSectorBitshift).Select(_ => reader.ReadSingle()).ToArray();
        SurfaceTypes = reader.ReadBytes(SurfaceTypesNum);
        reader.AlignTo(4);

        if (Size != 0)
        {
            Children = Array.Empty<STerrainNodeChunk>();
            return;
        }

        Children = Enumerable.Range(0, 4).Select(_ => new STerrainNodeChunk(reader, unitsToSectorBitshift)).ToArray();
    }
}