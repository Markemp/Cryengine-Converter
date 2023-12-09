using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "motion", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaMotion
{


    [XmlElement(ElementName = "instance_articulated_system")]
    public ColladaInstanceArticulatedSystem Instance_Articulated_System;

    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonMotion Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

