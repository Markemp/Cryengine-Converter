using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Rigid_Body : Grendgine_Collada_Technique_Common
	{
		
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

