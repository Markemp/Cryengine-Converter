using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Properties")]
public class EntityProperties
{
    #region Rise of Lyric specific attributes

    [XmlAttribute(AttributeName = "eiJumpPadType")]
    public string? JumpPadType;

    [XmlAttribute(AttributeName = "eiCrystalType")]
    public string? CrystalType;

    [XmlAttribute(AttributeName = "blades_Model")]
    public string? BladesModel;

    [XmlAttribute(AttributeName = "object_mesh")]
    public string? ObjectMesh;

    [XmlAttribute(AttributeName = "object_mesh_drc")]
    public string? ObjectMeshDrc;

    [XmlAttribute(AttributeName = "object_Model")]
    public string? ObjectModel;

    [XmlAttribute(AttributeName = "bUseLocalTrans")]
    public string? UseLocalTrans;

    [XmlAttribute(AttributeName = "bUseRotation")]
    public string? UseRotation;

    [XmlAttribute(AttributeName = "bUseTranslation")]
    public string? UseTranslation;

    [XmlElement(ElementName = "movement")] public RolXmlVector? Movement;

    [XmlElement(ElementName = "rotation")] public RolXmlVector? Rotation;

    public class RolXmlVector
    {
        [XmlAttribute(AttributeName = "x")] public string? X;

        [XmlAttribute(AttributeName = "y")] public string? Y;

        [XmlAttribute(AttributeName = "z")] public string? Z;
    }

    #endregion
}