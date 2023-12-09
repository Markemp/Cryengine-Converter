using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "frame_object", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public class ColladaFrameObject
{
    [XmlAttribute("link")]
    public string Link;

    [XmlElement(ElementName = "translate")]
    public ColladaTranslate[] Translate;

    [XmlElement(ElementName = "rotate")]
    public ColladaRotate[] Rotate;
}

