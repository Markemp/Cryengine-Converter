using System;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.collada.org/2005/11/COLLADASchema" )]
	public enum Grendgine_Collada_FX_Sampler_Common_Filter_Type
	{
		NONE,
		NEAREST,
		LINEAR,
		ANISOTROPIC
		
	}
}

