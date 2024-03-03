using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Articulated_Systems;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "motion", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

