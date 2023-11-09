using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Motion : ColladaTechniqueCommon
    {

        [XmlElement(ElementName = "axis_info")]
        public Grendgine_Collada_Axis_Info_Motion[] Axis_Info;

        [XmlElement(ElementName = "effector_info")]
        public Grendgine_Collada_Effector_Info Effector_Info;

    }
}

