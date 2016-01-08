using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[Serializable]
	[XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_Asset_Coverage
	{
	    [XmlElement(ElementName = "geographic_location")]
		Grendgine_Collada_Geographic_Location Geographic_Location;
	}
}

