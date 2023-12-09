using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

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

