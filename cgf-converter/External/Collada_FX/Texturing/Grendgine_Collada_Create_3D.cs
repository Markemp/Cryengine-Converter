using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="create_3d", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Create_3D
	{
		
		[XmlElement(ElementName = "size")]
		public Grendgine_Collada_Size_3D Size;				
		
		[XmlElement(ElementName = "mips")]
		public Grendgine_Collada_Mips_Attribute Mips;	
	
		
		[XmlElement(ElementName = "array")]
		public Grendgine_Collada_Array_Length Array_Length;		

		
		
		[XmlElement(ElementName = "format")]
		public Grendgine_Collada_Format Format;		
		
		[XmlElement(ElementName = "init_from")]
		public Grendgine_Collada_Init_From[] Init_From;		
				
	}
}

