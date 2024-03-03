using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaAssetCoverage
{
    [XmlElement(ElementName = "geographic_location")]
#pragma warning disable CS0169 // The field 'ColladaAssetCoverage.GeographicLocation' is never used
    readonly ColladaGeographicLocation Geographic_Location;
#pragma warning restore CS0169 // The field 'ColladaAssetCoverage.GeographicLocation' is never used
}

