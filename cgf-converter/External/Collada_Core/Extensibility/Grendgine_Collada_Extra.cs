using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Extra
	{

		[XmlAttribute("id")]
		public string ID;
		[XmlAttribute("name")]
		public string Name;
		[XmlAttribute("type")]
		public string Type;		
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;		
		
		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Technique[] Technique;		
	}
}

