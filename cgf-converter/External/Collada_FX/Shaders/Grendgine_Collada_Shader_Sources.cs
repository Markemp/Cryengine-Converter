using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="sources", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Shader_Sources
	{
		[XmlAttribute("entry")]
		public string Entry;	

	    [XmlElement(ElementName = "inline")]
		public string[] Inline;	

	    [XmlElement(ElementName = "import")]
		public Grendgine_Collada_Ref_String[] Import;	
		
		
	}
}

