using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Formula
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;		

		[XmlAttribute("sid")]
		public string sID;
	
		
	    [XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;
	    
		[XmlElement(ElementName = "technique_common")]
		public Grendgine_Collada_Technique_Common_Formula Technique_Common;
	    
		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Technique[] Technique;		
		
		
		[XmlElement(ElementName = "target")]
		public Grendgine_Collada_Common_Float_Or_Param_Type Target;		
		
	}
}

