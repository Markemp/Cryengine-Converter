using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Physics_Scene : Grendgine_Collada_Technique_Common
	{
		
		[XmlElement(ElementName = "gravity")]
		public Grendgine_Collada_SID_Float_Array_String Gravity;	

		[XmlElement(ElementName = "time_step")]
		public Grendgine_Collada_SID_Float Time_Step;		
	}
}

