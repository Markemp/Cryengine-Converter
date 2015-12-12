using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Int_Array : Grendgine_Collada_Int_Array_String
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("name")]
		public string Name;			
		
		[XmlAttribute("count")]
		public int Count;		
		
		[XmlAttribute("minInclusive")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(int), "-2147483648")]
		public int Min_Inclusive;		

		[XmlAttribute("maxInclusive")]
	    [System.ComponentModel.DefaultValueAttribute(typeof(int), "2147483647")]
		public int Max_Inclusive;		

	}
}

