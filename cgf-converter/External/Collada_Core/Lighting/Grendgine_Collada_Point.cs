using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Point
	{
		
		[XmlElement(ElementName = "color")]
		public Grendgine_Collada_Color Color;				
		
		[XmlElement(ElementName = "constant_attenuation")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(float), "1.0")]
		public Grendgine_Collada_SID_Float Constant_Attenuation;				
				
		[XmlElement(ElementName = "linear_attenuation")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(float), "0.0")]
		public Grendgine_Collada_SID_Float Linear_Attenuation;				

		[XmlElement(ElementName = "quadratic_attenuation")]
		public Grendgine_Collada_SID_Float Quadratic_Attenuation;
	}
}

