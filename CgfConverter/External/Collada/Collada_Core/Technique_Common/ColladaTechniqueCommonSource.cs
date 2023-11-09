using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaTechniqueCommonSource : Grendgine_Collada_Technique_Common
    {



        [XmlElement(ElementName = "accessor")]
        public ColladaAccessor Accessor;

        [XmlElement(ElementName = "asset")]
        public ColladaAsset Asset;
    }
}

