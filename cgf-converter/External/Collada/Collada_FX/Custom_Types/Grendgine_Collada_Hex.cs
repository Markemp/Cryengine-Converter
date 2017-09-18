using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[Serializable]
	[XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_Hex
	{
		[XmlAttribute("format")]
		public string Format;
		
		[XmlTextAttribute()]
	    public string Value;	
		//TODO: this is a hex array
	}
}

