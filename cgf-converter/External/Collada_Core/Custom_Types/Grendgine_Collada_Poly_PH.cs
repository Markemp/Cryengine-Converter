using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[Serializable]
	[XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_Poly_PH
	{

	    [XmlElement(ElementName = "p")]
		public Grendgine_Collada_Int_Array_String P;

		[XmlElement(ElementName = "h")]
		public Grendgine_Collada_Int_Array_String[] H;		


	}
}

