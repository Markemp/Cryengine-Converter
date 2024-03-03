using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Metadata;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Kinematics.Kinematics_Models;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "library_kinematics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaLibraryKinematicsModels
{
    [XmlAttribute("id")]
    public string ID;

    [XmlAttribute("name")]
    public string Name;


    [XmlElement(ElementName = "kinematics_model")]
    public ColladaKinematicsModel[] Kinematics_Model;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

