using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaStates
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

