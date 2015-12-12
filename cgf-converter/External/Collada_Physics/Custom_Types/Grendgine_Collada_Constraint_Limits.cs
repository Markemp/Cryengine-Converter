using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="limits", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Constraint_Limits
	{
		
		[XmlElement(ElementName = "swing_cone_and_twist")]
		public Grendgine_Collada_Constraint_Limit_Detail Swing_Cone_And_Twist;		
		
		[XmlElement(ElementName = "linear")]
		public Grendgine_Collada_Constraint_Limit_Detail Linear;		
		

	}
}

