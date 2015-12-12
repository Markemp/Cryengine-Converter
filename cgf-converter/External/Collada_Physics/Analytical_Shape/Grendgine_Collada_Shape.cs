using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="shape", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Shape
	{
		[XmlElement(ElementName = "hollow")]
		public Grendgine_Collada_SID_Bool Hollow;				
				
		[XmlElement(ElementName = "mass")]
		public Grendgine_Collada_SID_Float Mass;			
				
		[XmlElement(ElementName = "density")]
		public Grendgine_Collada_SID_Float Density;	


		[XmlElement(ElementName = "physics_material")]
		public Grendgine_Collada_Physics_Material Physics_Material;	

		[XmlElement(ElementName = "instance_physics_material")]
		public Grendgine_Collada_Instance_Physics_Material Instance_Physics_Material;	
		
		
		[XmlElement(ElementName = "instance_geometry")]
		public Grendgine_Collada_Instance_Geometry Instance_Geometry;	
		
		[XmlElement(ElementName = "plane")]
		public Grendgine_Collada_Plane Plane;	
		[XmlElement(ElementName = "box")]
		public Grendgine_Collada_Box Box;	
		[XmlElement(ElementName = "sphere")]
		public Grendgine_Collada_Sphere Sphere;	
		[XmlElement(ElementName = "cylinder")]
		public Grendgine_Collada_Cylinder Cylinder;	
		[XmlElement(ElementName = "capsule")]
		public Grendgine_Collada_Capsule Capsule;	
		
		
		
		[XmlElement(ElementName = "translate")]
		public Grendgine_Collada_Translate[] Translate;

		[XmlElement(ElementName = "rotate")]
		public Grendgine_Collada_Rotate[] Rotate;		
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;			
	}
}

