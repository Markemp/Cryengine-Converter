using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="convex_mesh", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Convex_Mesh
	{

		[XmlAttribute("convex_hull_of")]
		public string Convex_Hull_Of;		
		
	    [XmlElement(ElementName = "source")]
		public Grendgine_Collada_Source[] Source;		
		
	    [XmlElement(ElementName = "lines")]
		public Grendgine_Collada_Lines[] Lines;		
	    
		[XmlElement(ElementName = "linestrips")]
		public Grendgine_Collada_Linestrips[] Linestrips;		

	    [XmlElement(ElementName = "polygons")]
		public Grendgine_Collada_Polygons[] Polygons;		

	    [XmlElement(ElementName = "polylist")]
		public Grendgine_Collada_Polylist[] Polylist;		
		
	    [XmlElement(ElementName = "triangles")]
		public Grendgine_Collada_Triangles[] Triangles;		
		
	    [XmlElement(ElementName = "trifans")]
		public Grendgine_Collada_Trifans[] Trifans;		
		
	    [XmlElement(ElementName = "tristrips")]
		public Grendgine_Collada_Tristrips[] Tristrips;
		
		
	    [XmlElement(ElementName = "vertices")]
		public Grendgine_Collada_Vertices Vertices;		
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

