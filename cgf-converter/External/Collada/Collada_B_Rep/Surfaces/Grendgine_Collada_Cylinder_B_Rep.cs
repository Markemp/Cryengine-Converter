using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[Serializable]
	[XmlType(AnonymousType=true)]

	public partial class Grendgine_Collada_Cylinder_B_Rep
	{
	    [XmlElement(ElementName = "radius")]
		public Grendgine_Collada_Float_Array_String Radius;

		[XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
	}
}

