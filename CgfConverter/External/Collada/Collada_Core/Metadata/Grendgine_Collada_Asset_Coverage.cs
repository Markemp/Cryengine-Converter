using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Asset_Coverage
    {
        [XmlElement(ElementName = "geographic_location")]
#pragma warning disable CS0169 // The field 'Grendgine_Collada_Asset_Coverage.Geographic_Location' is never used
        readonly Grendgine_Collada_Geographic_Location Geographic_Location;
#pragma warning restore CS0169 // The field 'Grendgine_Collada_Asset_Coverage.Geographic_Location' is never used
    }
}

