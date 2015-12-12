using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Line
	{
		[XmlElement(ElementName = "origin")]
		public Grendgine_Collada_Origin Origin;
		
		[XmlElement(ElementName = "direction")]
		public Grendgine_Collada_Float_Array_String Direction;
				
		[XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

