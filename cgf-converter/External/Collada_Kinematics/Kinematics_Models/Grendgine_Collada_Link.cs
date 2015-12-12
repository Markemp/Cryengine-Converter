using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="link", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Link
	{
		[XmlAttribute("sid")]
		public string sID;
		[XmlAttribute("name")]
		public string Name;
		
		[XmlElement(ElementName = "translate")]
		public Grendgine_Collada_Translate[] Translate;

		[XmlElement(ElementName = "rotate")]
		public Grendgine_Collada_Rotate[] Rotate;		

		[XmlElement(ElementName = "attachment_full")]
		public Grendgine_Collada_Attachment_Full Attachment_Full;	

		[XmlElement(ElementName = "attachment_end")]
		public Grendgine_Collada_Attachment_End Attachment_End;	

		[XmlElement(ElementName = "attachment_start")]
		public Grendgine_Collada_Attachment_Start Attachment_Start;	
	}
}

