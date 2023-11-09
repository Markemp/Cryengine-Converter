using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Axis_Info_Motion : Grendgine_Collada_Axis_Info
    {

        [XmlElement(ElementName = "bind")]
        public Grendgine_Collada_Bind[] Bind;

        [XmlElement(ElementName = "newparam")]
        public ColladaNewParam[] New_Param;

        [XmlElement(ElementName = "setparam")]
        public ColladaNewParam[] Set_Param;

        [XmlElement(ElementName = "speed")]
        public Grendgine_Collada_Common_Float_Or_Param_Type Speed;

        [XmlElement(ElementName = "acceleration")]
        public Grendgine_Collada_Common_Float_Or_Param_Type Acceleration;

        [XmlElement(ElementName = "deceleration")]
        public Grendgine_Collada_Common_Float_Or_Param_Type Deceleration;

        [XmlElement(ElementName = "jerk")]
        public Grendgine_Collada_Common_Float_Or_Param_Type Jerk;



    }
}

