using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Asset
	{
		    [XmlElement(ElementName = "created")]
		    public System.DateTime Created;
		    
		    [XmlElement(ElementName = "modified")]
		    public System.DateTime Modified;
		
		    [XmlElement(ElementName = "unit")]
			public Grendgine_Collada_Asset_Unit Unit;
		
		    [XmlElement(ElementName = "up_axis")]
		    [System.ComponentModel.DefaultValueAttribute("Y_UP")]
			public string Up_Axis;
		
		    [XmlElement(ElementName = "contributor")]
			public Grendgine_Collada_Asset_Contributor[] Contributor;

		    [XmlElement(ElementName = "keywords")]
			public string Keywords;
		
		    [XmlElement(ElementName = "revision")]
			public string Revision;

			[XmlElement(ElementName = "subject")]
			public string Subject;
		    
			[XmlElement(ElementName = "title")]
			public string Title;	
		
		    [XmlElement(ElementName = "extra")]
			public Grendgine_Collada_Extra[] Extra;
		
		
		    [XmlElement(ElementName = "coverage")]
			public Grendgine_Collada_Asset_Coverage Coverage;
			
		
	}
}

