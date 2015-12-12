using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="profile_COMMON", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Profile_COMMON : Grendgine_Collada_Profile
	{

		[XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;

		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Effect_Technique_COMMON Technique;			
	}
}

