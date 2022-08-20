using System.Xml.Serialization;

namespace CgfConverter.Materials;

[XmlRoot(ElementName = "Material")]
public class Material
{
    private Color? diffuse;
    private Color? specular;
    private Color? emissive;
    private double? opacity;

    [XmlIgnore]
    internal string? SourceFileName { get; set; }

    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "MtlFlags")]
    public string? MtlFlags { get; set; }

    [XmlAttribute(AttributeName = "Shader")]
    public string? Shader { get; set; }

    [XmlAttribute(AttributeName = "GenMask")]
    public string? GenMask { get; set; }

    [XmlAttribute(AttributeName = "StringGenMask")]
    public string? StringGenMask { get; set; }

    [XmlAttribute(AttributeName = "SurfaceType")]
    public string? SurfaceType { get; set; }

    [XmlAttribute(AttributeName = "MatTemplate")]
    public string? MatTemplate { get; set; }

    [XmlAttribute(AttributeName = "Diffuse")]
    public string? Diffuse 
    { 
        get { return Color.Serialize(diffuse); }
        set { diffuse = Color.Deserialize(value); }
    }

    [XmlAttribute(AttributeName = "Specular")]
    public string? Specular 
    {
        get { return Color.Serialize(specular); }
        set { specular = Color.Deserialize(value); } 
    }

    [XmlAttribute(AttributeName = "Emissive")]
    public string? Emissive
    {
        get { return Color.Serialize(emissive);    ; }
        set { emissive = Color.Deserialize(value); }
    }

    [XmlAttribute(AttributeName = "Shininess")]
    public double Shininess { get; set; }

    [XmlAttribute(AttributeName = "Opacity")]
    public string? Opacity 
    { 
        get { return opacity.ToString(); }
        set { opacity = double.Parse(value ?? "1"); }
    }

    [XmlAttribute(AttributeName = "Glossiness")]
    public double Glossiness { get; set; }

    [XmlAttribute(AttributeName = "GlowAmount")]
    public double GlowAmount { get; set; }

    [XmlAttribute(AttributeName = "AlphaTest")]
    public double AlphaTest { get; set; }

    [XmlArray(ElementName = "SubMaterials")]
    [XmlArrayItem(ElementName = "Material")]
    public Material[] SubMaterials { get; set; }

    [XmlElement(ElementName = "PublicParams")]
    internal PublicParams PublicParams { get; set; }

    // TODO: TimeOfDay Support

    [XmlArray(ElementName = "Textures")]
    [XmlArrayItem(ElementName = "Texture")]
    public Texture[] Textures { get; set; }
}

[XmlRoot(ElementName = "xml")]
public class Xml
{
    [XmlElement(ElementName = "Material")]
    public Material? Material { get; set; }
}
