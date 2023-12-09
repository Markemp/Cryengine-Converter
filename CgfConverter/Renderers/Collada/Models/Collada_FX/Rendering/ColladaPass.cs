using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaPass
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlElement(ElementName = "annotate")]
    public ColladaAnnotate[] Annotate;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    [XmlElement(ElementName = "states")]
    public ColladaStates States;

    [XmlElement(ElementName = "evaluate")]
    public ColladaEffectTechniqueEvaluate Evaluate;
}

