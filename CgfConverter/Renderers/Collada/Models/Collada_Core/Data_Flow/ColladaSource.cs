using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Data_Flow;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaSource
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;

    [XmlElement(ElementName = "bool_array")]
    public ColladaBoolArray Bool_Array;
    [XmlElement(ElementName = "float_array")]
    public ColladaFloatArray Float_Array;
    [XmlElement(ElementName = "IDREF_array")]
    public ColladaIDREFArray IDREF_Array;
    [XmlElement(ElementName = "int_array")]
    public ColladaIntArray Int_Array;
    [XmlElement(ElementName = "Name_array")]
    public ColladaNameArray Name_Array;
    [XmlElement(ElementName = "SIDREF_array")]
    public ColladaSIDREFArray SIDREF_Array;
    [XmlElement(ElementName = "token_array")]
    public ColladaTokenArray Token_Array;

    [XmlElement(ElementName = "technique_common")]
    public ColladaTechniqueCommonSource Technique_Common;

    [XmlElement(ElementName = "technique")]
    public ColladaTechnique[] Technique;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    // ggerber 1.4.1 compatibilitiy
    [XmlAttribute("source")]
    public string Source;
}

