using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeographicLocationAltitude
{
    [XmlText()]
    public float Altitude;

    [XmlAttribute("mode")]
    [System.ComponentModel.DefaultValueAttribute(ColladaGeographicLocationAltitudeMode.relativeToGround)]
    public ColladaGeographicLocationAltitudeMode Mode;

}

