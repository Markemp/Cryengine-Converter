using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="kinematics", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Kinematics
	{
		
		[XmlElement(ElementName = "instance_kinematics_model")]
		public Grendgine_Collada_Instance_Kinematics_Model[] Instance_Kinematics_Model;

		
		[XmlElement(ElementName = "technique_common")]
		public Grendgine_Collada_Technique_Common_Kinematics Technique_Common;
	    
		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Technique[] Technique;			
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

