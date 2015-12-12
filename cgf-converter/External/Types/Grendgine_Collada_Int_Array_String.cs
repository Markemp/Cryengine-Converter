using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Int_Array_String
	{
		//TODO: cleanup to legit array

		[XmlTextAttribute()]
	    public string Value_As_String;
	}
}

