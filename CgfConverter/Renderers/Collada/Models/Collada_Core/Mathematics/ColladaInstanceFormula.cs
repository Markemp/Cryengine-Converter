using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Mathematics;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaInstanceFormula
{
    [XmlAttribute("sid")]
    public string sID;

    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("url")]
    public string URL;


    [XmlElement(ElementName = "setparam")]
    public ColladaSetParam[] Set_Param;
}

