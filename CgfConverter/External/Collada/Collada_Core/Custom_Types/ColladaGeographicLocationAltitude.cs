using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaGeographicLocationAltitude
    {

        [XmlTextAttribute()]
        public float Altitude;

        [XmlAttribute("mode")]
        [System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Geographic_Location_Altitude_Mode.relativeToGround)]
        public Grendgine_Collada_Geographic_Location_Altitude_Mode Mode;

    }
}

