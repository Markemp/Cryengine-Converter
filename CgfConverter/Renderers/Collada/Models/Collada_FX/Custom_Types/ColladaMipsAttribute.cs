using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaMipsAttribute
{
    [XmlAttribute("levels")]
    public int Levels;

    [XmlAttribute("auto_generate")]
    public bool Auto_Generate;
}

