using System;
using System.IO;
using Extensions;

namespace CgfConverter.Terrain;

public class STerrainInfo
{
    public readonly int HeightMapSizeInUnits;
    public readonly int UnitSizeInMeters;
    public readonly int SectorSizeInMeters;
    public readonly int SectorsTableSizeInSectors;
    public readonly float HeightmapZRatio;
    public readonly float OceanWaterLevel;

    public STerrainInfo(BinaryReader reader, int version)
    {
        switch (version)
        {
            case 24:
                reader.ReadInto(out HeightMapSizeInUnits);
                reader.ReadInto(out UnitSizeInMeters);
                reader.ReadInto(out SectorSizeInMeters);
                reader.ReadInto(out SectorsTableSizeInSectors);
                reader.ReadInto(out HeightmapZRatio);
                reader.ReadInto(out OceanWaterLevel);
                return;
        }

        throw new NotSupportedException();
    }

    public int UnitsToSectorBitshift
    {
        get
        {
            var res = 0;
            var v = SectorSizeInMeters / UnitSizeInMeters;
            while (v > 1)
            {
                res += 1;
                v >>= 1;
            }

            return res;
        }
    }
}