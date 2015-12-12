using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Geometry
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;	
		
		
		[XmlElement(ElementName = "brep")]
		public Grendgine_Collada_B_Rep B_Rep;

		[XmlElement(ElementName = "convex_mesh")]
		public Grendgine_Collada_Convex_Mesh Convex_Mesh;

		
		[XmlElement(ElementName = "spline")]
		public Grendgine_Collada_Spline Spline;

		[XmlElement(ElementName = "mesh")]
		public Grendgine_Collada_Mesh Mesh;
		
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;	
	}
}

