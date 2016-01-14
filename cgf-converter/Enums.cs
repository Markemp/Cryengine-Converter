namespace CgfConverter
{
    public enum FileVersionEnum : uint
    {
        CryTek_3_4 = 0x744,
        CryTek_3_5 = 0x745,
        CryTek_3_6 = 0x746,
    }
    // Enums
    public enum FileTypeEnum : uint
    {
        GEOM = 0xFFFF0000,
        ANIM = 0xFFFF0001
    }  // complete
    public enum MtlNameTypeEnum : uint
    {
        Library = 0x01,
        Single = 0x10,
        Child = 0x12,
    }
    public enum ChunkTypeEnum : uint    // complete
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
        SourceInfo = 0xCCCC0013, //Describes the source from which the cgf was exported: source max file, machine and user.
        MtlName = 0xCCCC0014, //provides material name as used in the material.xml file
        ExportFlags = 0xCCCC0015, //Describes export information.
        DataStream = 0xCCCC0016, //A data Stream
        MeshSubsets = 0xCCCC0017, //Describes an array of mesh subsets
        MeshPhysicsData = 0xCCCC0018, //Physicalized mesh data
        CompiledBones = 0xACDC0000, //unknown chunk
        CompiledPhysicalBones = 0xACDC0001, // unknown chunk
        CompiledMorphtargets = 0xACDC0002,  // unknown chunk
        CompiledPhysicalProxies = 0xACDC0003, //unknown chunk
        CompiledIntFaces = 0xACDC0004, //unknown chunk
        CompiledIntSkinVertices = 0xACDC0004, //unknown chunk
        CompiledExt2IntMap = 0xACDC0005, //unknown chunk
        BreakablePhysics = 0xACDC0006, //unknown chunk
        FaceMap = 0xAAFC0000, //unknown chunk
        SpeedInfo = 0xAAFC0002, //Speed and distnace info
        FootPlantInfo = 0xAAFC0003, // Footplant info
        BonesBoxes = 0xAAFC0004, // unknown chunk
        UnknownAAFC0005 = 0xAAFC0005 //unknown chunk
    }

    public enum ChunkTypetmp
    {
        Any,
        Mesh,
        Helper,
        VertAnim,
        BoneAnim,
        GeomNameList,
        BoneNameList,
        MtlList,
        MRM,
        SceneProps,
        Light,
        PatchMesh,
        Node,
        Mtl,
        Controller,
        Timing,
        BoneMesh,
        BoneLightBinding,
        MeshMorphTarget,
        BoneInitialPos,
        SourceInfo,
        MtlName,
        ExportFlags,
        DataStream,
        MeshSubsets,
        MeshPhysicsData,
        CompiledBones,
        CompiledPhysicalBones,
        CompiledMorphtargets,
        CompiledPhysicalProxies,
        CompiledIntFaces,
        CompiledIntSkinVertices,
        CompiledExt2IntMap,
        BreakablePhysics,
        FaceMap,
        SpeedInfo,
        FootPlantInfo,
        BonesBoxes,
        UnknownAAFC0005
    }
    public enum ChunkType36    // complete, but never used.
    {
        Any = 0x0,
        Mesh = 0x1000,
        Helper = 0x1001,
        VertAnim = 0x1002,
        BoneAnim = 0x1003,
        GeomNameList = 0x1004,
        BoneNameList = 0x1005,
        MtlList = 0x1006,
        MRM = 0x1007, //obsolete
        SceneProps = 0x1008,
        Light = 0x1009,
        PatchMesh = 0x100A,
        Node = 0x100B,
        Mtl = 0x100C,
        Controller = 0x100D,
        Timing = 0x100E,
        BoneMesh = 0x100F,
        BoneLightBinding = 0x1010,
        MeshMorphTarget = 0x1011,
        BoneInitialPos = 0x1012,
        SourceInfo = 0x1013, //Describes the source from which the cgf was exported: source max file, machine and user.
        MtlName = 0x1014, //provides material name as used in the material.xml file
        ExportFlags = 0x1015, //Describes export information.
        DataStream = 0x1016, //A data Stream
        MeshSubsets = 0x1017, //Describes an array of mesh subsets
        MeshPhysicalData = 0x1018, //Physicalized mesh data
        // not sure what the following enums will be, since not experienced yet.
        /*CompiledBones = 0xACDC0000, //unknown chunk
        CompiledPhysicalBones = 0xACDC0001, // unknown chunk
        CompiledMorphtargets = 0xACDC0002,  // unknown chunk
        CompiledPhysicalProxies = 0xACDC0003, //unknown chunk
        CompiledIntFaces = 0xACDC0004, //unknown chunk
        CompiledIntSkinVertices = 0xACDC0004, //unknown chunk
        CompiledExt2IntMap = 0xACDC0005, //unknown chunk
        BreakablePhysics = 0xACDC0006, //unknown chunk
        FaceMap = 0xAAFC0000, //unknown chunk
        SpeedInfo = 0xAAFC0002, //Speed and distnace info
        FootPlantInfo = 0xAAFC0003, // Footplant info
        BonesBoxes = 0xAAFC0004, // unknown chunk
        UnknownAAFC0005 = 0xAAFC0005 //unknown chunk*/
    }

    public enum ChunkVersion : uint
    {
        ChkVersion
    }    //complete
    public enum ChunkVersion36 : short
    {
        ChkVersion36
    }
    public enum HelperTypeEnum : uint
    {
        POINT,
        DUMMY,
        XREF,
        CAMERA,
        GEOMETRY
    }      //complete
    public enum MtlType : uint            //complete
    {
        UNKNOWN,
        STANDARD,
        MULTI,
        TWOSIDED
    }
    public enum MtlNamePhysicsType : uint //complete
    {
        NONE = 0xFFFFFFFF,
        DEFAULT = 0x00000000,
        NOCOLLIDE = 0x00000001,
        OBSTRUCT = 0x00000002,
        DEFAULTPROXY = 0x000000FF,  // this needs to be checked.  cgf.xml says 256; not sure if hex or dec
        UNKNOWN = 0x00001100,
    }
    public enum LightType : uint         //complete
    {
        OMNI,
        SPOT,
        DIRECT,
        AMBIENT
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
    public enum DataStreamTypeEnum : uint
    {
        VERTICES,
        NORMALS,
        UVS,
        COLORS,
        COLORS2,
        INDICES,
        TANGENTS,
        SHCOEFFS,
        SHAPEDEFORMATION,
        BONEMAP,
        FACEMAP,
        VERTMATS,
        UNKNOWN1,
        UNKNOWN2,
        UNKNOWN3,
        VERTSUVS,
        UNKNOWN5,
        UNKNOWN6
    }  //complete
    public enum PhysicsPrimitiveType : uint
    {
        CUBE = 0X0,
        POLYHEDRON = 0X1,
        CYLINDER = 0X5,
        UNKNOWN6 = 0X6   // nothing between 2-4, no idea what unknown is.
    }
}