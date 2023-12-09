using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Parameters;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.COMMON
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "profile_COMMON", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class ColladaProfileCOMMON : ColladaProfile
    {

        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "technique")]
        public ColladaEffectTechniqueCOMMON Technique;
    }
}

