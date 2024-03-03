using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "link", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaLink
{
    [XmlAttribute("sid")]
    public string sID;
    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "translate")]
    public ColladaTranslate[] Translate;

    [XmlElement(ElementName = "rotate")]
    public ColladaRotate[] Rotate;

    [XmlElement(ElementName = "attachment_full")]
    public ColladaAttachmentFull Attachment_Full;

    [XmlElement(ElementName = "attachment_end")]
    public ColladaAttachmentEnd Attachment_End;

    [XmlElement(ElementName = "attachment_start")]
    public ColladaAttachmentStart Attachment_Start;
}

