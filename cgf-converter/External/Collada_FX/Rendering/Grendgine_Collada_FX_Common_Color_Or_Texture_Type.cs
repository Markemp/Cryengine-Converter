using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="fx_common_color_or_texture_type", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_FX_Common_Color_Or_Texture_Type
	{
		[XmlAttribute("opaque")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Opaque_Channel.A_ONE)]		
		public Grendgine_Collada_FX_Opaque_Channel Opaque;

		[XmlElement(ElementName = "param")]
		public Grendgine_Collada_Param Param;			
		
		[XmlElement(ElementName = "color")]
		public Grendgine_Collada_Color Color;			
		
		[XmlElement(ElementName = "texture")]
		public Grendgine_Collada_Texture Texture;			
	}
}

