using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Axis_Info_Motion : Grendgine_Collada_Axis_Info
	{
		
		[XmlElement(ElementName = "bind")]
		public Grendgine_Collada_Bind[] Bind;	

		[XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;	

		[XmlElement(ElementName = "setparam")]
		public Grendgine_Collada_New_Param[] Set_Param;	
		
		[XmlElement(ElementName = "speed")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Speed;	

		[XmlElement(ElementName = "acceleration")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Acceleration;	

		[XmlElement(ElementName = "deceleration")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Deceleration;	

		[XmlElement(ElementName = "jerk")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Jerk;	

		
		
	}
}

