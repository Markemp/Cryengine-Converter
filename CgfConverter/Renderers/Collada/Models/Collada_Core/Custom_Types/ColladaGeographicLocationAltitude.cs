using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Enums;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Custom_Types;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaGeographicLocationAltitude
{
    [XmlText()]
    public float Altitude;

    [XmlAttribute("mode")]
    [System.ComponentModel.DefaultValue(ColladaGeographicLocationAltitudeMode.relativeToGround)]
    public ColladaGeographicLocationAltitudeMode Mode;

}

