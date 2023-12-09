using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "rigid_constraint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaRigidConstraint
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "ref_attachment")]
    public ColladaRefAttachment Ref_Attachment;

    [XmlElement(ElementName = "attachment")]
    public ColladaAttachment Attachment;


    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonRigidConstraint Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

