using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_SID_Int_Array_String
	{
		[XmlAttribute("sid")]
		public string sID;

		[XmlTextAttribute()]
	    public string Value_As_String;
	}
}

