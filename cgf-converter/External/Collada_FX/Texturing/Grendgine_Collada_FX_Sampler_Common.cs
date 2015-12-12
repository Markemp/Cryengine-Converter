using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="fx_sampler_common", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_FX_Sampler_Common
	{

		[XmlElement(ElementName = "texcoord")]
		public Grendgine_Collada_TexCoord_Semantic TexCoord_Semantic;		
			
		[XmlElement(ElementName = "wrap_s")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
		public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_S;		
		
		[XmlElement(ElementName = "wrap_t")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
		public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_T;		
		
		[XmlElement(ElementName = "wrap_p")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
		public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_P;		
		
		[XmlElement(ElementName = "minfilter")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
		public Grendgine_Collada_FX_Sampler_Common_Filter_Type MinFilter;		
		
		[XmlElement(ElementName = "magfilter")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
		public Grendgine_Collada_FX_Sampler_Common_Filter_Type MagFilter;		
		
		[XmlElement(ElementName = "mipfilter")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
		public Grendgine_Collada_FX_Sampler_Common_Filter_Type MipFilter;		
		
		[XmlElement(ElementName = "border_color")]
		public Grendgine_Collada_Float_Array_String Border_Color;		
		
		[XmlElement(ElementName = "mip_max_level")]
		public byte Mip_Max_Level;		
		
		[XmlElement(ElementName = "mip_min_level")]
		public byte Mip_Min_Level;		
		
		[XmlElement(ElementName = "mip_bias")]
		public float Mip_Bias;		
		
		[XmlElement(ElementName = "max_anisotropy")]
		public int Max_Anisotropy;		
		
		
		[XmlElement(ElementName = "instance_image")]
		public Grendgine_Collada_Instance_Image Instance_Image;		
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;				
	}
}

