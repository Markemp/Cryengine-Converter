using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="motion", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Motion
	{

		
		[XmlElement(ElementName = "instance_articulated_system")]
		public Grendgine_Collada_Instance_Articulated_System Instance_Articulated_System;
		
		[XmlElement(ElementName = "technique_common")]
		public Grendgine_Collada_Technique_Common_Motion Technique_Common;
	    
		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Technique[] Technique;			
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra; 
	}
}

