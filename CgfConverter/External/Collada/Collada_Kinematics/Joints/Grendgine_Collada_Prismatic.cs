using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "prismatic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Prismatic
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlElement(ElementName = "axis")]
        public Grendgine_Collada_SID_Float_Array_String Axis;

        [XmlElement(ElementName = "limits")]
        public Grendgine_Collada_Kinematics_Limits Limits;


    }
}

