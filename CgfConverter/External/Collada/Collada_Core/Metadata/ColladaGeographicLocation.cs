using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeographicLocation
{
    [XmlElement(ElementName = "longitude")]
    public float Longitude;

    [XmlElement(ElementName = "latitude")]
    public float Latitude;

    [XmlElement(ElementName = "altitude")]
    public ColladaGeographicLocationAltitude Altitude;
}

