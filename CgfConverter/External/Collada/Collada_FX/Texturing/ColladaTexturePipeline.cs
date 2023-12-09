using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "texture_pipeline", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

