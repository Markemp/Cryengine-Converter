using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="limits", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Kinematics_Limits
	{
	    [XmlElement(ElementName = "min")]
		public Grendgine_Collada_SID_Name_Float Min;	
	    [XmlElement(ElementName = "max")]
		public Grendgine_Collada_SID_Name_Float Max;			
	}
}

