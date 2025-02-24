using System;

namespace CgfConverter;

public enum FileVersion : uint
{
    Unknown = 0,
    x0744 = 0x744,
    x0745 = 0x745,
    x0746 = 0x746,
    x0900 = 0x900
}

public enum FileType : uint
{
    Geometry = 0xFFFF0000,
    Animation = 0xFFFF0001
}  // complete

[Flags]
public enum MtlNameType : uint
{
    // TODO: This is a bitwise struct. 
    Basic = 0x00,
    Library = 0x01,     // multi material
    MwoChild = 0x02,    // Submaterial
    Single = 0x10,
    Child = 0x12,
    Unknown1 = 0x0B,   // Collision materials?  In MWO, these are the torsos, arms, legs from body/<mech>.mtl
    Unknown2 = 0x04
}

[Flags]
enum MaterialTypeFlags : uint
{
    FLAG_MULTI_MATERIAL = 0x0001, // Have sub materials info.
    FLAG_SUB_MATERIAL = 0x0002, // This is sub material.
    FLAG_SH_COEFFS = 0x0004, // This material should get spherical harmonics coefficients computed.
    FLAG_SH_2SIDED = 0x0008, // This material will be used as 2 sided in the sh precomputation
    FLAG_SH_AMBIENT = 0x0010, // This material will get an ambient sh term(to not shadow it entirely)
};

public enum ChunkType : uint    // complete
{
    Any = 0x0,
    Mesh = 0xCCCC0000,
    Helper = 0xCCCC0001,
    VertAnim = 0xCCCC0002,
    BoneAnim = 0xCCCC0003,
    GeomNameList = 0xCCCC0004,
    BoneNameList = 0xCCCC0005,
    MtlList = 0xCCCC0006,
    MRM = 0xCCCC0007, //obsolete
    SceneProps = 0xCCCC0008,
    Light = 0xCCCC0009,
    PatchMesh = 0xCCCC000A,
    Node = 0xCCCC000B,
    Mtl = 0xCCCC000C,
    Controller = 0xCCCC000D,
    Timing = 0xCCCC000E,
    BoneMesh = 0xCCCC000F,
    BoneLightBinding = 0xCCCC0010,
    MeshMorphTarget = 0xCCCC0011,
    BoneInitialPos = 0xCCCC0012,
    SourceInfo = 0xCCCC0013,        // Describes the source from which the cgf was exported: source max file, machine and user.
    MtlName = 0xCCCC0014,           // provides material name as used in the material.xml file
    ExportFlags = 0xCCCC0015,       // Describes export information.
    DataStream = 0xCCCC0016,        // A data Stream
    MeshSubsets = 0xCCCC0017,       // Describes an array of mesh subsets
    MeshPhysicsData = 0xCCCC0018,   // Physicalized mesh data
    CompiledBones = 0xACDC0000,
    CompiledPhysicalBones = 0xACDC0001,
    CompiledMorphTargets = 0xACDC0002,
    CompiledPhysicalProxies = 0xACDC0003,
    CompiledIntFaces = 0xACDC0004,
    CompiledIntSkinVertices = 0xACDC0005,
    CompiledExt2IntMap = 0xACDC0006,
    BreakablePhysics = 0xACDC0007,
    FaceMap = 0xAAFC0000,           // unknown chunk
    SpeedInfo = 0xAAFC0002,         // Speed and distnace info
    FootPlantInfo = 0xAAFC0003,     // Footplant info
    BonesBoxes = 0xAAFC0004,        // unknown chunk
    FoliageInfo = 0xAAFC0005,       // unknown chunk
    GlobalAnimationHeaderCAF = 0xAAFC0007,
    
    // Star Citizen versions
    NodeSC = 0xCCCC100B,
    CompiledBonesSC = 0xCCCC1000,
    CompiledPhysicalBonesSC = 0xCCCC1001,
    CompiledMorphTargetsSC = 0xCCCC1002,
    CompiledPhysicalProxiesSC = 0xCCCC1003,
    CompiledIntFacesSC = 0xCCCC1004,
    CompiledIntSkinVerticesSC = 0xCCCC1005,
    CompiledExt2IntMapSC = 0xCCCC1006,
    UnknownSC1 = 0xCCCC2004,
    // Star Citizen #ivo file chunks
    NodeMeshCombo = 0x70697FDA,
    MtlNameIvo = 0x8335674E,
    MtlNameIvo320 = 0x83353333,
    CompiledPhysicalBonesIvo = 0x90C687DC,  // Physics
    CompiledPhysicalBonesIvo320 = 0x90C66666,
    MeshInfo = 0x92914444,
    MeshIvo = 0x9293B9D8,           // SkinInfo
    IvoSkin = 0xB875B2D9,           // SkinMesh
    IvoSkin2 = 0xB8757777,
    CompiledBones_Ivo = 0xC201973C,     // cgf/cga files with multiple nodes. 0x900 and 0x901
    CompiledBones_Ivo2 = 0xC2011111,    // 3.24 and newer SC
    BShapesGPU = 0x57A3BEFD,
    BShapes = 0x875CCB28,

    BinaryXmlDataSC = 0xcccbf004,
}

public enum HelperType : uint
{
    POINT,
    DUMMY,
    XREF,
    CAMERA,
    GEOMETRY
}

public enum MtlNamePhysicsType : uint //complete
{
    NONE = 0xFFFFFFFF,
    DEFAULT = 0x00000000,
    NOCOLLIDE = 0x00000001,
    OBSTRUCT = 0x00000002,
    DEFAULTPROXY = 0x000000FF,  // this needs to be checked.  cgf.xml says 256; not sure if hex or dec
    UNKNOWN = 0x00001100,       // collision mesh?
    UNKNOWN2 = 0x00001000
}

public enum LightType : uint         //complete
{
    OMNI,
    SPOT,
    DIRECT,
    AMBIENT
}

public enum IvoGeometryType : short
{
    Geometry = 0x0,
    Helper2 = 0x2,
    Helper3 = 0x3       // Have only seen 0 (geometry) and 2/3 (helper) in the wild.
}

public enum CtrlType : uint
{
    NONE,
    CRYBONE,
    LINEAR1,
    LINEAR3,
    LINEARQ,
    BEZIER1,
    BEZIER3,
    BEZIERQ,
    TBC1,
    TBC3,
    TBCQ,
    BSPLINE2O,
    BSPLINE1O,
    BSPLINE2C,
    BSPLINE1C,
    CONST          // this was given a value of 11, which is the same as BSPLINE2o.
}        //complete

public enum TextureMapping : uint
{
    NORMAL,
    ENVIRONMENT,
    SCREENENVIRONMENT,
    CUBIC,
    AUTOCUBIC
}  //complete

public enum DatastreamType : uint
{
    VERTICES,
    NORMALS,
    UVS,
    COLORS,
    COLORS2,
    INDICES,    // 0x05
    TANGENTS,
    DUMMY0,
    DUMMY1,
    BONEMAP,    // 0x09
    FACEMAP,
    VERTMATS,
    QTANGENTS,   // Prey Normals?
    SKINDATA,
    DUMMY2,
    VERTSUVS,   // 0x0F
    NUMTYPES,
    IVONORMALS = 0x9CF3F615,
    IVONORMALS2 = 0x38A581FE,           // ResourceFiles\SC\ivo\new_skin_format\Avenger_Landing_Gear\AEGS_Vanguard_LandingGear_Front.skinm
    IVOCOLORS2 = 0xD9EED421,           // Objects\Characters\Human\male_v7\armor\ccc\m_ccc_vanduul_helmet_01.skinm
    IVOINDICES = 0xEECDC168,
    IVOTANGENTS = 0xB95E9A1B,
    IVOQTANGENTS = 0xEE057252,
    IVOBONEMAP = 0x677C7B23,
    IVOVERTSUVS = 0x91329AE9,
    IVOVERTSUVS2 = 0xB3A70D5E,
    IVOBONEMAP32 = 0x6ECA3708,           // Objects\Characters\Human\heads\male\npc\male01\male01_t2_head.skinm
    IVOUNKNOWN = 0x9D51C5EE,        // box.cgfm.  2 bytes, all zeros, numvertices
}

public enum PhysicsPrimitiveType : uint
{
    CUBE = 0X0,
    POLYHEDRON = 0X1,
    CYLINDER = 0X5,
    UNKNOWN6 = 0X6   // nothing between 2-4, no idea what unknown is.
}

public enum XmlFileType
{
    MATERIAL,
    PREFAB,
    CHRPARAMS
}

/// <summary>
/// Flags for the mesh. How to check:
///   var flags = EFlags.MESH_IS_EMPTY | EFlags.HAS_TEX_MAPPING_DENSITY;
///   bool hasTexMappingDensity = (flags & EFlags.HAS_TEX_MAPPING_DENSITY) != 0;
/// </summary>
[Flags]
public enum MeshChunkFlag
{
    MESH_IS_EMPTY = 0x0001, // Empty mesh (no datastreams.  geometry may be in geometry model)
    HAS_TEX_MAPPING_DENSITY = 0x0002, // has texMappingDensity
    HAS_EXTRA_WEIGHTS = 0x0004, // Bonemap stream has weights 5-8
    HAS_FACE_AREA = 0x0008, // has geometricMeanFaceArea
};

public enum VertexFormat : uint
{
    eVF_Unknown,

    // Base stream
    eVF_P3F_C4B_T2F,
    eVF_P3F_C4B_T2F_T2F,
    eVF_P3S_C4B_T2S,
    eVF_P3S_C4B_T2S_T2S, // For UV2 support  
    eVF_P3S_N4B_C4B_T2S,

    eVF_P3F_C4B_T4B_N3F2, // Particles.
    eVF_TP3F_C4B_T2F, // Fonts (28 bytes).
    eVF_TP3F_T2F_T3F,  // Miscellaneus.
    eVF_P3F_T3F,       // Miscellaneus. (AuxGeom)
    eVF_P3F_T2F_T3F,   // Miscellaneus.

    // Additional streams
    eVF_T2F,           // Light maps TC (8 bytes).
    eVF_W4B_I4S,  // Skinned weights/indices stream.
    eVF_C4B_C4B,      // SH coefficients.
    eVF_P3F_P3F_I4B,  // Shape deformation stream.
    eVF_P3F,       // Velocity stream.

    eVF_C4B_T2S,     // General (Position is merged with Tangent stream)

    // Lens effects simulation
    eVF_P2F_T4F_C4F,  // primary
    eVF_P2F_T4F_T4F_C4F,

    eVF_P2S_N4B_C4B_T1F,
    eVF_P3F_C4B_T2S,
    eVF_P2F_C4B_T2F_F4B, // UI
    eVF_P3F_C4B,// Auxiliary geometry

    eVF_P3F_C4F_T2F  //numbering for tracking the new vertex formats and for comparison with testing 23
    // There are more 
};
