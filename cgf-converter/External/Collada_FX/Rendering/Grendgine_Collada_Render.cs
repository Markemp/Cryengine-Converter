using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="render", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Render
	{
		[XmlAttribute("name")]
		public string Name;	
		
		[XmlAttribute("sid")]
		public string sid;
		
		[XmlAttribute("camera_node")]
		public string Camera_Node;
		
	    [XmlElement(ElementName = "layer")]
		public string[] Layer;			
		
	    [XmlElement(ElementName = "instance_material")]
		public Grendgine_Collada_Instance_Material_Rendering Instance_Material;			
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;			
	}
}

