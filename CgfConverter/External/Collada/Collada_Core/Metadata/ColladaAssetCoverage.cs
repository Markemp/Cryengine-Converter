using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAssetCoverage
{
    [XmlElement(ElementName = "geographic_location")]
#pragma warning disable CS0169 // The field 'Grendgine_Collada_Asset_Coverage.Geographic_Location' is never used
    readonly ColladaGeographicLocation Geographic_Location;
#pragma warning restore CS0169 // The field 'Grendgine_Collada_Asset_Coverage.Geographic_Location' is never used
}

