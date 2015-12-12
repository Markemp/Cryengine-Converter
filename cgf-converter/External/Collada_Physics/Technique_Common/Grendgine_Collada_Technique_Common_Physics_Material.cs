using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Physics_Material : Grendgine_Collada_Technique_Common
	{
		
		[XmlElement(ElementName = "dynamic_friction")]
		public Grendgine_Collada_SID_Float Dynamic_Friction;	
		
		[XmlElement(ElementName = "restitution")]
		public Grendgine_Collada_SID_Float Restitution;	
		
		[XmlElement(ElementName = "static_friction")]
		public Grendgine_Collada_SID_Float Static_Friction;			
	}
}

