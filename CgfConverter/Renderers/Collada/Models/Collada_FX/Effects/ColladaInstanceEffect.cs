using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "instance_effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaInstanceEffect
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("url")]
        public string URL;

        [XmlElement(ElementName = "setparam")]
        public ColladaSetParam[] Set_Param;

        [XmlElement(ElementName = "extra")]
        public ColladaExtra[] Extra;

        [XmlElement(ElementName = "technique_hint")]
        public ColladaTechniqueHint[] Technique_Hint;

    }
}

