using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Technique_Common_Source : Grendgine_Collada_Technique_Common
	{
	
		
		
		[XmlElement(ElementName = "accessor")]
		public Grendgine_Collada_Accessor Accessor;	
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;	
	}
}

