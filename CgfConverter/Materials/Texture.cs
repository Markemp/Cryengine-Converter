using System.ComponentModel;
using System.Xml.Serialization;
using System;
using CgfConverter.Materials;

/// <summary>The texture object</summary>
[XmlRoot(ElementName = "Texture")]
public class Texture
{
    public enum TypeEnum
    {
        [XmlEnum("0")]
        Default = 0,
        [XmlEnum("3")]
        Environment = 3,
        [XmlEnum("5")]
        Interface = 5,
        [XmlEnum("7")]
        CubeMap = 7,
        [XmlEnum("Nearest Cube-Map probe for alpha blended")]
        NearestCubeMap = 8
    }

    public enum MapTypeEnum
    {
        Unknown = 0,
        Diffuse,
        Bumpmap,
        Specular,
        Environment,
        Decal,
        SubSurface,
        Custom,
        Opacity,
        Detail,
        Heightmap,
        BlendDetail,
    }

    [XmlAttribute(AttributeName = "Map")]
    public string __Map
    {
        get { return Enum.GetName(typeof(MapTypeEnum), Map); }
        set
        {
            _ = Enum.TryParse(value, out MapTypeEnum buffer);
            Map = buffer;
        }
    }

    /// <summary>Diffuse, Specular, Bumpmap, Environment, HeightMamp or Custom</summary>
    [XmlIgnore]
    public MapTypeEnum Map { get; set; }

    /// <summary>Location of the texture</summary>
    [XmlAttribute(AttributeName = "File")]
    public string File { get; set; }

    /// <summary>The type of the texture</summary>
    [XmlAttribute(AttributeName = "TexType")]
    [DefaultValue(TypeEnum.Default)]
    public TypeEnum TexType;

    /// <summary>The modifier to apply to the texture</summary>
    [XmlElement(ElementName = "TexMod")]
    public TextureModifier Modifier;
}