using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="instance_material", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Instance_Material_Geometry
	{
		[XmlAttribute("sid")]
		public string sID;
		
		[XmlAttribute("name")]
		public string Name;		
		
		[XmlAttribute("target")]
		public string Target;	
		
		[XmlAttribute("symbol")]
		public string Symbol;	
		
	    [XmlElement(ElementName = "bind")]
		public Grendgine_Collada_Bind_FX[] Bind;	
		
	    [XmlElement(ElementName = "bind_vertex_input")]
		public Grendgine_Collada_Bind_Vertex_Input[] Bind_Vertex_Input;	
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;		
	}
}

