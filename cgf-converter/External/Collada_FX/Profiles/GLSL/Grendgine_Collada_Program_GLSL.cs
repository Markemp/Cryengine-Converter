using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="program", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Program_GLSL
	{

	    [XmlElement(ElementName = "shader")]
		public Grendgine_Collada_Shader_GLSL[] Shader;	
		
	    [XmlElement(ElementName = "bind_attribute")]
		public Grendgine_Collada_Bind_Attribute[] Bind_Attribute;			

	    [XmlElement(ElementName = "bind_uniform")]
		public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;	
	}
}

