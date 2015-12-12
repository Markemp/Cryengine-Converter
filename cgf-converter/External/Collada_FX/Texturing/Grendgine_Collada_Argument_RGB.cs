using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="argument", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Argument_RGB
	{
		[XmlAttribute("source")]
		public Grendgine_Collada_Argument_Source Source;

		[XmlAttribute("operand")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Argument_RGB_Operand.SRC_COLOR)]
		public Grendgine_Collada_Argument_RGB_Operand Operand;
				
		[XmlAttribute("sampler")]
		public string Sampler;
		
	}
}

