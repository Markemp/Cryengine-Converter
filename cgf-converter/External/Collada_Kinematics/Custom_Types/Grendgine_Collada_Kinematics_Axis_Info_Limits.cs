using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Kinematics_Axis_Info_Limits
	{
	    [XmlElement(ElementName = "min")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Min;	
	    [XmlElement(ElementName = "max")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Max;
	}
}

