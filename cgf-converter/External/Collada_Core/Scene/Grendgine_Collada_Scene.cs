using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Scene
	{
		
		[XmlElement(ElementName = "instance_visual_scene")]
		public Grendgine_Collada_Instance_Visual_Scene Visual_Scene;
			
		[XmlElement(ElementName = "instance_physics_scene")]
		public Grendgine_Collada_Instance_Physics_Scene[] Physics_Scene;

		[XmlElement(ElementName = "instance_kinematics_scene")]
		public Grendgine_Collada_Instance_Kinematics_Scene Kinematics_Scene;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;		
	}
}

