using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Accessor
	{
		[XmlAttribute("count")]
		public uint Count;

		[XmlAttribute("offset")]
		public uint Offset;		
		
		[XmlAttribute("source")]
		public string Source;		
		
		[XmlAttribute("stride")]
		public uint Stride;		
		
	    [XmlElement(ElementName = "param")]
		public Grendgine_Collada_Param[] Param;				
	}
}

