using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="instance_kinematics_scene", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Instance_Kinematics_Scene
	{
		[XmlAttribute("sid")]
		public string sID;

		[XmlAttribute("name")]
		public string Name;

		[XmlAttribute("url")]
		public string URL;	
		
		[XmlElement(ElementName = "asset")]
		public Grendgine_Collada_Asset Asset;
		
	    [XmlElement(ElementName = "extra")]
		public Grendgine_Collada_Extra[] Extra;
		
	    [XmlElement(ElementName = "newparam")]
		public Grendgine_Collada_New_Param[] New_Param;

		[XmlElement(ElementName = "setparam")]
		public Grendgine_Collada_Set_Param[] Set_Param;
		
		
		[XmlElement(ElementName = "bind_kinematics_model")]
		public Grendgine_Collada_Bind_Kinematics_Model[] Bind_Kenematics_Model;

		[XmlElement(ElementName = "bind_joint_axis")]
		public Grendgine_Collada_Bind_Joint_Axis[] Bind_Joint_Axis;
	}
}

