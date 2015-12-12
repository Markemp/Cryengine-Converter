using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Perspective
	{
		[XmlElement(ElementName = "xfov")]
		public Grendgine_Collada_SID_Float XFov;				
				
		[XmlElement(ElementName = "yfov")]
		public Grendgine_Collada_SID_Float YFov;				

		[XmlElement(ElementName = "aspect_ratio")]
		public Grendgine_Collada_SID_Float Aspect_Ratio;				

		[XmlElement(ElementName = "znear")]
		public Grendgine_Collada_SID_Float ZNear;				

		[XmlElement(ElementName = "zfar")]
		public Grendgine_Collada_SID_Float ZFar;	
	}
}

