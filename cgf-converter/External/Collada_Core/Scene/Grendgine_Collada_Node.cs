using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	public partial class Grendgine_Collada_Node
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("sid")]
		public string sID;

		[XmlAttribute("name")]
		public string Name;				

		[XmlAttribute("type")]
		[System.ComponentModel.DefaultValueAttribute(Grendgine_Collada_Node_Type.NODE)]
		public Grendgine_Collada_Node_Type Type;				

		[XmlAttribute("layer")]
		public string Layer;				
		
		[XmlElement(ElementName = "lookat")]
		public Grendgine_Collada_Lookat[] Lookat;

		[XmlElement(ElementName = "matrix")]
		public Grendgine_Collada_Matrix[] Matrix;

		[XmlElement(ElementName = "rotate")]
		public Grendgine_Collada_Rotate[] Rotate;

		[XmlElement(ElementName = "scale")]
		public Grendgine_Collada_Scale[] Scale;

		[XmlElement(ElementName = "skew")]
		public Grendgine_Collada_Skew[] Skew;

		[XmlElement(ElementName = "translate")]
		public Grendgine_Collada_Translate[] Translate;
		
		[XmlElement(ElementName = "instance_camera")]
		public Grendgine_Collada_Instance_Camera[] Instance_Camera;
		
		[XmlElement(ElementName = "instance_controller")]
		public Grendgine_Collada_Instance_Controller[] Instance_Controller;
		
		[XmlElement(ElementName = "instance_geometry")]
		public Grendgine_Collada_Instance_Geometry[] Instance_Geometry;
		
		[XmlElement(ElementName = "instance_light")]
		public Grendgine_Collada_Instance_Light[] Instance_Light;
		
		[XmlElement(ElementName = "instance_node")]
		public Grendgine_Collada_Instance_Node[] Instance_Node;

		
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;
		
	    [XmlElement(ElementName = "node")]
		public Grendgine_Collada_Node[] node;		
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;		

	}
}

