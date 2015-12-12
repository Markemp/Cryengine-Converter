using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="compiler", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Compiler
	{
		[XmlAttribute("platform")]
		public string Platform;

		[XmlAttribute("target")]
		public string Target;
	
		[XmlAttribute("options")]
		public string Options;
	
		[XmlElement(ElementName = "binary")]
		public Grendgine_Collada_Binary Binary;		
	}
}

