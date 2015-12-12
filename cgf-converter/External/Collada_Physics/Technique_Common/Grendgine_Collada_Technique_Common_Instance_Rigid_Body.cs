using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="technique_common", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Technique_Common_Instance_Rigid_Body : Grendgine_Collada_Technique_Common
	{

		[XmlElement(ElementName = "angular_velocity")]
		public Grendgine_Collada_Float_Array_String Angular_Velocity;

		[XmlElement(ElementName = "velocity")]
		public Grendgine_Collada_Float_Array_String Velocity;
		
		[XmlElement(ElementName = "dynamic")]
		public Grendgine_Collada_SID_Bool Dynamic;
		
		[XmlElement(ElementName = "mass")]
		public Grendgine_Collada_SID_Float Mass;		
		
		[XmlElement(ElementName = "inertia")]
		public Grendgine_Collada_SID_Float_Array_String Inertia;		
		
		[XmlElement(ElementName = "mass_frame")]
		public Grendgine_Collada_Mass_Frame Mass_Frame;		
		
		
		[XmlElement(ElementName = "physics_material")]
		public Grendgine_Collada_Physics_Material Physics_Material;		
		
		[XmlElement(ElementName = "instance_physics_material")]
		public Grendgine_Collada_Instance_Physics_Material Instance_Physics_Material;		
		
		
		[XmlElement(ElementName = "shape")]
		public Grendgine_Collada_Shape[] Shape;		
	}
}

