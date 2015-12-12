using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Geographic_Location_Altitude
	{

		[XmlTextAttribute()]
		public float Altitude;
		
		[XmlAttribute("mode")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Geographic_Location_Altitude_Mode.relativeToGround)]
		public Grendgine_Collada_Geographic_Location_Altitude_Mode Mode;	
		
	}
}

