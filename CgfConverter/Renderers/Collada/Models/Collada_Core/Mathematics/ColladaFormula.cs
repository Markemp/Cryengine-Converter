using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Mathematics;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaFormula
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("sid")]
    public string sID;


    [XmlElement(ElementName = "newparam")]
    public ColladaNewParam[] New_Param;

    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonFormula Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;


    [XmlElement(ElementName = "target")]
    public ColladaCommonFloatOrParamType Target;

}

