using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_B_Rep
	{
		
				
		[XmlElement(ElementName = "curves")]
		public Grendgine_Collada_Curves Curves;
				
		[XmlElement(ElementName = "surface_curves")]
		public Grendgine_Collada_Surface_Curves Surface_Curves;
				
		[XmlElement(ElementName = "surfaces")]
		public Grendgine_Collada_Surfaces Surfaces;
		
		[XmlElement(ElementName = "source")]
		public Grendgine_Collada_Source[] Source;
		
		[XmlElement(ElementName = "vertices")]
		public Grendgine_Collada_Vertices Vertices;
		
		
		[XmlElement(ElementName = "edges")]
		public Grendgine_Collada_Edges Edges;
		
		[XmlElement(ElementName = "wires")]
		public Grendgine_Collada_Wires Wires;
		
		[XmlElement(ElementName = "faces")]
		public Grendgine_Collada_Faces Faces;
		
		[XmlElement(ElementName = "pcurves")]
		public Grendgine_Collada_PCurves PCurves;
		
		[XmlElement(ElementName = "shells")]
		public Grendgine_Collada_Shells Shells;
		
		[XmlElement(ElementName = "solids")]
		public Grendgine_Collada_Solids Solids;

		
		
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;					
		
	}
}

