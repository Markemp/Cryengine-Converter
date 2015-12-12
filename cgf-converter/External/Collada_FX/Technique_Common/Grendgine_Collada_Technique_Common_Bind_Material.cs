using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{

	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="technique_common", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Technique_Common_Bind_Material : Grendgine_Collada_Technique_Common
	{
	    
		[XmlElement(ElementName = "instance_material")]
		public Grendgine_Collada_Instance_Material_Geometry[] Instance_Material;	
		
	}
}

