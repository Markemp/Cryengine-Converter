using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="effect", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Effect
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;		
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;	
		
		[XmlElement(ElementName = "annotate")]
		public Grendgine_Collada_Annotate[] Annotate;
		
	    [XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;

		[XmlElement(ElementName = "profile_BRIDGE")]
		public Grendgine_Collada_Profile_BRIDGE[] Profile_BRIDGE;
		
		[XmlElement(ElementName = "profile_CG")]
		public Grendgine_Collada_Profile_CG[] Profile_CG;
		
		[XmlElement(ElementName = "profile_GLES")]
		public Grendgine_Collada_Profile_GLES[] Profile_GLES;
		
		[XmlElement(ElementName = "profile_GLES2")]
		public Grendgine_Collada_Profile_GLES2[] Profile_GLES2;
		
		[XmlElement(ElementName = "profile_GLSL")]
		public Grendgine_Collada_Profile_GLSL[] Profile_GLSL;
		
		[XmlElement(ElementName = "profile_COMMON")]
		public Grendgine_Collada_Profile_COMMON[] Profile_COMMON;
				
	}
}

