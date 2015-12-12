using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]

	public partial class Grendgine_Collada_Axis_Info_Kinematics : Grendgine_Collada_Axis_Info
	{
		[XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;	
		
		[XmlElement(ElementName = "active")]
		public Grendgine_Collada_Common_Bool_Or_Param_Type Active;	
		
		[XmlElement(ElementName = "locked")]
		public Grendgine_Collada_Common_Bool_Or_Param_Type Locked;	
		
		[XmlElement(ElementName = "index")]
		public Grendgine_Collada_Kinematics_Axis_Info_Index[] Index;	
		
		[XmlElement(ElementName = "limits")]
		public Grendgine_Collada_Kinematics_Axis_Info_Limits Limits;	
		
		[XmlElement(ElementName = "formula")]
		public Grendgine_Collada_Formula[] Formula;	
		
		[XmlElement(ElementName = "instance_formula")]
		public Grendgine_Collada_Instance_Formula[] Instance_Formula;	
	}
}

