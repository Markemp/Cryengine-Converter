using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[Serializable]
	[XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_SID_Float
	{
		[XmlAttribute("sid")]
		public string sID;
		
	    [XmlTextAttribute()]
	    public float Value;		
		
	}
}

