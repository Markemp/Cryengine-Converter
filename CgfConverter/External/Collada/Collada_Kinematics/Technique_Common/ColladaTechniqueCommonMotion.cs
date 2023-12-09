using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueCommonMotion : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "axis_info")]
    public ColladaAxisInfoMotion[] Axis_Info;

    [XmlElement(ElementName = "effector_info")]
    public ColladaEffectorInfo Effector_Info;

}

