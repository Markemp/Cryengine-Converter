using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Visual_Scene
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;	
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;			
		
	    [XmlElement(ElementName = "evaluate_scene")]
		public Grendgine_Collada_Evaluate_Scene[] Evaluate_Scene;			

		
	    [XmlElement(ElementName = "node")]
		public Grendgine_Collada_Node[] Node;			
		
	}
}

