using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaInputUnshared
{
    [XmlAttribute("semantic")]
    // Commenting out default value as it won't write.
    //[System.ComponentModel.DefaultValueAttribute(ColladaInputSemantic.NORMAL)]
    public ColladaInputSemantic Semantic;

    [XmlAttribute("source")]
    public string source;

}

