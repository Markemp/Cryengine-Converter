using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="shader", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Shader_CG : Grendgine_Collada_Shader
	{

	    [XmlElement(ElementName = "bind_uniform")]
		public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;	
		
	    [XmlElement(ElementName = "compiler")]
		public Grendgine_Collada_Compiler[] Compiler;		
	}
}

