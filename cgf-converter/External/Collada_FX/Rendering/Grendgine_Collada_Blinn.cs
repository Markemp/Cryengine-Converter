using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="blinn", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Blinn
	{
		
		[XmlElement(ElementName = "emission")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Eission;		
		
		[XmlElement(ElementName = "ambient")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Ambient;		
		
		[XmlElement(ElementName = "diffuse")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Diffuse;		
		
		[XmlElement(ElementName = "specular")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Specular;		
		
		[XmlElement(ElementName = "transparent")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Transparent;		
		
		[XmlElement(ElementName = "reflective")]
		public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Reflective;		

	
	
		[XmlElement(ElementName = "shininess")]
		public Grendgine_Collada_FX_Common_Float_Or_Param_Type Shininess;		
		
		[XmlElement(ElementName = "reflectivity")]
		public Grendgine_Collada_FX_Common_Float_Or_Param_Type Reflectivity;		
		
		[XmlElement(ElementName = "transparency")]
		public Grendgine_Collada_FX_Common_Float_Or_Param_Type Transparency;		
		
		[XmlElement(ElementName = "index_of_refraction")]
		public Grendgine_Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;		
	
	
	}
}

