using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="image", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Image
	{
		[XmlAttribute("id")]
		public string ID;
		
		[XmlAttribute("sid")]
		public string sID;
		
		[XmlAttribute("name")]
		public string Name;		

	    [XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;			
		
	    [XmlElement(ElementName = "renderable")]
		public Grendgine_Collada_Renderable_Share Renderable_Share;			
		
	    [XmlElement(ElementName = "init_from")]
		public Grendgine_Collada_Init_From Init_From;			
		
	    [XmlElement(ElementName = "create_2d")]
		public Grendgine_Collada_Create_2D Create_2D;			

	    [XmlElement(ElementName = "create_3d")]
		public Grendgine_Collada_Create_3D Create_3D;			

	    [XmlElement(ElementName = "create_cube")]
		public Grendgine_Collada_Create_Cube Create_Cube;			
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;			
	}
}

