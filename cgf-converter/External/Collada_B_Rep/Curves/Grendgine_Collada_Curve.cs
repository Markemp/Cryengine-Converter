using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Curve
	{
		[XmlAttribute("sid")]
		public string sID;
		
		[XmlAttribute("name")]
		public string Name;			
		
		[XmlElement(ElementName = "line")]
		public Grendgine_Collada_Line Line;
		
		[XmlElement(ElementName = "circle")]
		public Grendgine_Collada_Circle Circle;
		
		[XmlElement(ElementName = "ellipse")]
		public Grendgine_Collada_Ellipse Ellipse;
		
		[XmlElement(ElementName = "parabola")]
		public Grendgine_Collada_Parabola Parabola;
		
		[XmlElement(ElementName = "hyperbola")]
		public Grendgine_Collada_Hyperbola Hyperbola;
		
		[XmlElement(ElementName = "nurbs")]
		public Grendgine_Collada_Nurbs Nurbs;
		
		
	    [XmlElement(ElementName = "orient")]
		public Grendgine_Collada_Orient[] Orient;			
		
		[XmlElement(ElementName = "origin")]
		public Grendgine_Collada_Origin Origin;
	}
}

