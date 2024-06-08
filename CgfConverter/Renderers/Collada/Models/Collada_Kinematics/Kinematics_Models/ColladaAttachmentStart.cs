using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "attachment_start", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaAttachmentStart
{
    [XmlAttribute("joint")]
    public string Joint;

    [XmlElement(ElementName = "translate")]
    public ColladaTranslate[] Translate;

    [XmlElement(ElementName = "rotate")]
    public ColladaRotate[] Rotate;
}

