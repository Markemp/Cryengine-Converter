using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="instance_articulated_system", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Instance_Articulated_System
	{
		[XmlAttribute("sid")]
		public string sID;

		[XmlAttribute("name")]
		public string Name;

		[XmlAttribute("url")]
		public string URL;	
		
	    [XmlElement(ElementName = "bind")]
		public Grendgine_Collada_Bind[] Bind;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
		
	    [XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;

		[XmlElement(ElementName = "setparam")]
		public Grendgine_Collada_Set_Param[] Set_Param;		
		
	}
}

