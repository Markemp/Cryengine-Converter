using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Optics : Grendgine_Collada_Technique_Common
	{
		
		[XmlElement(ElementName = "orthographic")]
		public Grendgine_Collada_Orthographic Orthographic;

		[XmlElement(ElementName = "perspective")]
		public Grendgine_Collada_Perspective Perspective;	
	}
}

