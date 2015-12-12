using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
	[System.Xml.Serialization.XmlRootAttribute(ElementName="states", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_States
	{

		
		//TODO: clean these up, its a massive list
/// <summary>
/// Needs some runtime cleanup: valid attributes of each element are value, index, and param. the element name is the type
/// <states>
/// 	<fog_color value="0 0 0 0" /> 
/// 	<fog_enable = "true"/> 
/// 	<light_ambient value="1 1 1 0" index="0"/> 
/// 	<light_diffuse param="someparam" />
/// </states>
/// </summary>	
		
		[XmlAnyElement]
		public XmlElement[] Data;	
		
	}
}

