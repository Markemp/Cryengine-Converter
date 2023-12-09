using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonPhysicsScene : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "gravity")]
    public ColladaSIDFloatArrayString Gravity;

    [XmlElement(ElementName = "time_step")]
    public ColladaSIDFloat Time_Step;
}

