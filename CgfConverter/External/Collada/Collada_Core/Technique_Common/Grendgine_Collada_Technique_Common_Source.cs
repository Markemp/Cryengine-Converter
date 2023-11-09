using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Source : Grendgine_Collada_Technique_Common
    {



        [XmlElement(ElementName = "accessor")]
        public Grendgine_Collada_Accessor Accessor;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;
    }
}

