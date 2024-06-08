using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaKinematicsLimits
{
    [XmlElement(ElementName = "min")]
    public ColladaSIDNameFloat Min;
    [XmlElement(ElementName = "max")]
    public ColladaSIDNameFloat Max;
}

