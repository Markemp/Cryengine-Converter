using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="joint", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Joint
	{
		[XmlAttribute("id")]
		public string ID;

		[XmlAttribute("name")]
		public string Name;

		[XmlAttribute("sid")]
		public string sID;	
		
	    [XmlElement(ElementName = "prismatic")]
		public Grendgine_Collada_Prismatic[] Prismatic;		
	    
		[XmlElement(ElementName = "revolute")]
		public Grendgine_Collada_Revolute[] Revolute;		
	    
		[XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;		
	}
}

