using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Mips_Attribute
	{
		
		[XmlAttribute("levels")]
		public int Levels;
		
		[XmlAttribute("auto_generate")]
		public bool Auto_Generate;	
	}
}

