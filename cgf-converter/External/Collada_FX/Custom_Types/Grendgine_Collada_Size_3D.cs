using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="size", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Size_3D
	{


		[XmlAttribute("width")]
		public int Width;	

		[XmlAttribute("height")]
		public int Height;	

		[XmlAttribute("depth")]
		public int Depth;			
	}
}

