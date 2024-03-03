using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Texturing;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "texture_pipeline", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTexturePipeline
{

    [XmlAttribute("sid")]
    public string sID;


    [XmlElement(ElementName = "texcombiner")]
    public ColladaTexCombiner[] TexCombiner;

    [XmlElement(ElementName = "texenv")]
    public ColladaTexEnv[] TexEnv;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

}

