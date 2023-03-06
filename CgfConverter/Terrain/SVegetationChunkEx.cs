using System;
using System.IO;
using System.Numerics;
using Extensions;

namespace CgfConverter.Terrain;

public class SVegetationChunkEx : SVegetationChunk
{
    // Cryengine3: (x - 127) / 127   [-1...1]
    private readonly byte NormalX;
    private readonly byte NormalY;

    // Lumberyard: x * pi / 255      [0...pi]   
    private readonly byte AngleX;
    private readonly byte AngleY;

    public SVegetationChunkEx(BinaryReader reader, int version) : base(reader, version)
    {
        switch (version)
        {
            case 4:
                reader.ReadInto(out NormalX);
                reader.ReadInto(out NormalY);
                return;
            case 5:
                reader.ReadInto(out AngleX);
                reader.ReadInto(out AngleY);
                return;
        }

        throw new NotSupportedException();
    }

    public Quaternion GetRotation(StatInstGroupChunk group, float vegetationAlignToTerrainAmount = 1f)
    {
        switch (ChunkVersion)
        {
            case 4:
            {
                if ((group.Flags & StatInstGroupChunkFlags.AlignToTerrain) == 0)
                    return Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationZ(AngleZ));
                
                var x = (NormalX - 127) / 127f;
                var y = (NormalY - 127) / 127f;
                var z = MathF.Sqrt(1f - x * x + y * y);
                // Note: "z = sqrtf(1 - x*x + y*y)" is probably wrong
                var terrainNormal = new Vector3(x, y, z);
                var vDir = Vector3.Cross(-Vector3.UnitX, terrainNormal);
                var terrainAlignmentMatrix =
                    Matrix4x4Extensions.CreateOrientation(vDir, -terrainNormal, 0);
                var rotationMatrix = terrainAlignmentMatrix * Matrix4x4.CreateRotationZ(AngleZ);
                return Quaternion.CreateFromRotationMatrix(rotationMatrix);
            }
            case 5:
            {
                var alignToTerrainAmount = group.AlignToTerrainCoefficient * vegetationAlignToTerrainAmount;
                if (alignToTerrainAmount == 0)
                    return Quaternion.CreateFromRotationMatrix(
                        Matrix4x4Extensions.CreateFromAngles(AngleX, AngleY, AngleZ));

                throw new NotSupportedException();
                // var terrainNormal = GetTerrainNormal(alignToTerrainAmount);
                // var vDir = Vector3.Cross(-Vector3.UnitX, terrainNormal);
                // var terrainAlignmentMatrix = Matrix4x4Extensions.CreateOrientation(vDir, -terrainNormal, alignToTerrainAmount);
                // var rotationMatrix = terrainAlignmentMatrix * Matrix4x4.CreateRotationZ(AngleZ);
                // return Quaternion.CreateFromRotationMatrix(rotationMatrix);
            }
            default:
                throw new NotSupportedException();
        }
    }
}