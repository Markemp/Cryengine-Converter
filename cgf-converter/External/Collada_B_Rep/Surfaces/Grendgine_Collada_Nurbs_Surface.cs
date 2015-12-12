using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Nurbs_Surface
	{
		[XmlAttribute("degree_u")]
		public int Degree_U;
		[XmlAttribute("closed_u")]
		public bool Closed_U;		
		[XmlAttribute("degree_v")]
		public int Degree_V;
		[XmlAttribute("closed_v")]
		public bool Closed_V;		
		
		[XmlElement(ElementName = "source")]
		public Grendgine_Collada_Source[] Source;		
		
		[XmlElement(ElementName = "control_vertices")]
		public Grendgine_Collada_Control_Vertices Control_Vertices;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;			
	}
}

