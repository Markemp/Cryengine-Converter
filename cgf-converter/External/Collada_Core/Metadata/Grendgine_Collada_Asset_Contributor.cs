
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Asset_Contributor
	{



	    [XmlElement(ElementName = "author")]
		public string Author;
		
	    [XmlElement(ElementName = "author_email")]
		public string Author_Email;
		
	    [XmlElement(ElementName = "author_website")]
		public string Author_Website;
		
	    [XmlElement(ElementName = "authoring_tool")]
		public string Authoring_Tool;
		
	    [XmlElement(ElementName = "comments")]
		public string Comments;
		
	    [XmlElement(ElementName = "copyright")]
		public string Copyright;
		
	    [XmlElement(ElementName = "source_data")]
		public string Source_Data;		
		
	}
}

