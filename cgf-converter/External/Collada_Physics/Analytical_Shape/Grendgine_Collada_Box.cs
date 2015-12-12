using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="box", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Box
	{
		[XmlElement(ElementName = "half_extents")]
		public Grendgine_Collada_Float_Array_String Half_Extents;		

	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

