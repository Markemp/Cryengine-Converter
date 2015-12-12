using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Light : Grendgine_Collada_Technique_Common
	{
		[XmlElement(ElementName = "ambient")]
		public Grendgine_Collada_Ambient Ambient;		
		
		[XmlElement(ElementName = "directional")]
		public Grendgine_Collada_Directional Directional;		
		
		[XmlElement(ElementName = "point")]
		public Grendgine_Collada_Point Point;		
		
		[XmlElement(ElementName = "spot")]
		public Grendgine_Collada_Spot Spot;		
		
		
		
		
		
		
	}
}

