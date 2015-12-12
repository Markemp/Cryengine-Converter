using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="annotate", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_RGB
	{
		[XmlAttribute("operator")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_RGB_Operator.ADD)]
		public Grendgine_Collada_RGB_Operator Operator;	

		[XmlAttribute("scale")]
		public float Scale;	
		
	    [XmlElement(ElementName = "argument")]
		public Grendgine_Collada_Argument_RGB[] Argument;			
	}
}

