using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="technique", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Effect_Technique_COMMON : Grendgine_Collada_Effect_Technique
	{
		
		[XmlElement(ElementName = "blinn")]
		public Grendgine_Collada_Blinn Blinn;
		
		[XmlElement(ElementName = "constant")]
		public Grendgine_Collada_Constant Constant;
		
		[XmlElement(ElementName = "lambert")]
		public Grendgine_Collada_Lambert Lambert;
		
		[XmlElement(ElementName = "phong")]
		public Grendgine_Collada_Phong Phong;		
	}
}

