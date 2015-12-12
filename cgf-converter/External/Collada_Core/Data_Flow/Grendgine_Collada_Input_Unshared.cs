using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Input_Unshared
	{
		[XmlAttribute("semantic")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Input_Semantic.NORMAL)]
		public Grendgine_Collada_Input_Semantic Semantic;	

		[XmlAttribute("source")]
		public string source;

	}
}

