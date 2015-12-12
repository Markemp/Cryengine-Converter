using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="index", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Kinematics_Axis_Info_Index : Grendgine_Collada_Common_Int_Or_Param_Type
	{
		
		[XmlAttribute("semantic")]
		public string Semantic;

		
	}
}

