using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="rigid_constraint", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Rigid_Constraint
	{
		[XmlAttribute("sid")]
		public string sID;
		
		[XmlAttribute("name")]
		public string Name;	

		
		[XmlElement(ElementName = "ref_attachment")]
		public Grendgine_Collada_Ref_Attachment Ref_Attachment;
		
		[XmlElement(ElementName = "attachment")]
		public Grendgine_Collada_Attachment Attachment;
		
		
		[XmlElement(ElementName = "technique_common")]
		public Grendgine_Collada_Technique_Common_Rigid_Constraint Technique_Common;
	    
		[XmlElement(ElementName = "technique")]
		public Grendgine_Collada_Technique[] Technique;			
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

