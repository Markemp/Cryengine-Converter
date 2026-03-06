using System.IO;

namespace CgfConverter.CryEngineCore;

/// <summary>
/// Controller chunk version 0x828 - Empty/unsupported format.
///
/// In the Lumberyard source (CryHeaders.h), CONTROLLER_CHUNK_DESC_0828 is defined as an
/// empty struct with no fields. The engine logs a warning and skips it entirely.
/// We read no data and produce no bone tracks.
/// </summary>
internal sealed class ChunkController_828 : ChunkController
{
    public override void Read(BinaryReader b)
    {
        // 0x828 has no data fields — initialise from chunk table header only.
        ChunkType = _header.ChunkType;
        VersionRaw = _header.VersionRaw;
        Offset = _header.Offset;
        ID = _header.ID;
        Size = _header.Size;
        DataSize = Size;

        Utilities.HelperMethods.Log(Utilities.LogLevelEnum.Warning,
            $"ChunkController_828 (ID={ID:X}) is not supported by CryEngine and will be skipped.");
    }

    public override string ToString() =>
        $"ChunkController_828: ID={ID:X} (unsupported/empty)";
}
