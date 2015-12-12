using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Input_Shared : Grendgine_Collada_Input_Unshared
	{
		[XmlAttribute("offset")]
		public int Offset;
		
		[XmlAttribute("set")]
		public int Set;				
		
	}
}

