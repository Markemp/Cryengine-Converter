using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Geographic_Location
	{


	    [XmlElement(ElementName = "longitude")]
		public float Longitude;
	    
		[XmlElement(ElementName = "latitude")]
		public float Latitude;
		
	    [XmlElement(ElementName = "altitude")]
		public Grendgine_Collada_Geographic_Location_Altitude Altitude;		
		
	}
}

