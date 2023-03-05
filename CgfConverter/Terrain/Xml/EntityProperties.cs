using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

[XmlRoot(ElementName = "Properties")]
public class EntityProperties
{
    [XmlAttribute(AttributeName = "object_Model")]
    public string? ObjectModel;

    [XmlAttribute(AttributeName = "bUseLocalTrans")]
    public string? UseLocalTrans;

    [XmlAttribute(AttributeName = "bUseRotation")]
    public string? UseRotation;
    
    [XmlAttribute(AttributeName = "bUseTranslation")]
    public string? UseTranslation;

    [XmlElement(ElementName = "movement")] public XmlVector? Movement;
    
    [XmlElement(ElementName = "rotation")] public XmlVector? Rotation;

    public class XmlVector
    {
        [XmlAttribute(AttributeName = "x")]
        public string? X;
        
        [XmlAttribute(AttributeName = "y")]
        public string? Y;
        
        [XmlAttribute(AttributeName = "z")]
        public string? Z;
    }
}