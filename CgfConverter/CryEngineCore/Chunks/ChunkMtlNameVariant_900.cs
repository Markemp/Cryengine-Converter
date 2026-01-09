using System;
using System.IO;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Parser for IvoMtlNameVariant chunk (0x83353533) in Star Citizen 4.5+ files.
/// This chunk has the same structure as MtlNameIvo320 (0x83353333) and represents
/// material variant/override information.
/// </summary>
internal sealed class ChunkMtlNameVariant_900 : ChunkMtlName
{
    /// <summary>
    /// Material indices used by this mesh (references into SubMaterials array).
    /// These are the MatIDs that geometry subsets reference.
    /// </summary>
    public ushort[] MaterialIndices { get; private set; } = [];

    public override void Read(BinaryReader b)
    {
        base.Read(b);
        
        // 128 bytes: Material file path (null-terminated, padded)
        Name = b.ReadFString(128);
        
        // 4 bytes: Number of material indices
        NumChildren = b.ReadUInt32();
        
        // 32 bytes: Reserved/padding (all zeros)
        SkipBytes(b, 32);
        
        // NumChildren * 2 bytes: Material indices (uint16 each)
        // These are indices into the SubMaterials array of the referenced material file
        MaterialIndices = new ushort[NumChildren];
        for (int i = 0; i < NumChildren; i++)
        {
            MaterialIndices[i] = b.ReadUInt16();
        }
        
        // Set MatType based on whether we have children
        MatType = NumChildren == 0 ? MtlNameType.Single : MtlNameType.Library;
    }
}
