using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="spring", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Constraint_Spring
	{
		
		[XmlElement(ElementName = "linear")]
		public Grendgine_Collada_Constraint_Spring_Type Linear;	
		
		[XmlElement(ElementName = "angular")]
		public Grendgine_Collada_Constraint_Spring_Type Angular;			
		
	}
}

