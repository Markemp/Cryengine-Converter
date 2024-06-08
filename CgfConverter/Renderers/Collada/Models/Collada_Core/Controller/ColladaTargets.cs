using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Controller;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTargets
{

    [XmlElement(ElementName = "input")]
    public ColladaInputUnshared[] Input;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

