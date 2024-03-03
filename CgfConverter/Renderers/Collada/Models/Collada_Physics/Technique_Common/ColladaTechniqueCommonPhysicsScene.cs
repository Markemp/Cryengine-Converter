using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonPhysicsScene : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "gravity")]
    public ColladaSIDFloatArrayString Gravity;

    [XmlElement(ElementName = "time_step")]
    public ColladaSIDFloat Time_Step;
}

