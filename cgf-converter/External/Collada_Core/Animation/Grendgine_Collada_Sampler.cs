using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Sampler
	{
		[XmlAttribute("id")]
		public string ID;
	
		[XmlAttribute("pre_behavior")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Sampler_Behavior.UNDEFINED)]
		public Grendgine_Collada_Sampler_Behavior Pre_Behavior;

		[XmlAttribute("post_behavior")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Sampler_Behavior.UNDEFINED)]
		public Grendgine_Collada_Sampler_Behavior Post_Behavior;
		
		
		[XmlElement(ElementName = "input")]
		public Grendgine_Collada_Input_Unshared[] Input;			
	}
}

