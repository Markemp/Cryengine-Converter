using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

[Serializable]
[XmlType(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "library_kinematics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
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

