using System.Xml.Serialization;

namespace CgfConverter.Models.Materials;

/// <summary>
/// Path to a material file with a single material
/// </summary>
[XmlRoot(ElementName = "MaterialRef")]
public class MaterialRef : MaterialBase
{

}
