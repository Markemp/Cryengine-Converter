using System.ComponentModel;
using System.Xml.Serialization;

namespace CgfConverter.Materials;

/// <summary>The texture modifier</summary>
[XmlRoot(ElementName = "TexMod")]
public class TextureModifier
{
    [XmlAttribute(AttributeName = "TexMod_RotateType")]
    public int RotateType { get; set; }

    [XmlAttribute(AttributeName = "TexMod_TexGenType")]
    public int GenType { get; set; }

    [XmlAttribute(AttributeName = "TexMod_bTexGenProjected")]
    [DefaultValue(1)]
    public int __Projected
    {
        get { return this.Projected ? 1 : 0; }
        set { Projected = value == 1; }
    }

    [XmlIgnore]
    public bool Projected { get; set; }

    [XmlAttribute(AttributeName = "TileU")]
    [DefaultValue(0)]
    public double TileU { get; set; }

    [XmlAttribute(AttributeName = "TileV")]
    [DefaultValue(0)]
    public double TileV { get; set; }

    [XmlAttribute(AttributeName = "OffsetU")]
    [DefaultValue(0)]
    public double OffsetU { get; set; }
}
