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
        Diffuse,
        Normals,
        Specular,
        Env,
        DetailOverlay,
        SecondSmoothness,
        Height,
        DecalOverlay,
        Subsurface,
        Custom,
        CustomSecondary,
        Opacity,
        Smoothness,
        Emittance,
        Occlusion,
        Specular2,
        Unknown,
    }

    [XmlAttribute(AttributeName = "Map")]
    public string MapString = string.Empty;
    
    /// <summary>Diffuse, Specular, Bumpmap, Environment, HeightMamp or Custom</summary>
    [XmlIgnore]
    public MapTypeEnum Map
    {
        get => MapString switch
        {
            "Diffuse" => MapTypeEnum.Diffuse,
            "Bumpmap" => MapTypeEnum.Normals,
            "Specular" => MapTypeEnum.Specular,
            "Environment" => MapTypeEnum.Env,
            "Detail" => MapTypeEnum.DetailOverlay,
            "SecondSmoothness" => MapTypeEnum.SecondSmoothness,
            "Heightmap" => MapTypeEnum.Height,
            "Decal" => MapTypeEnum.DecalOverlay,
            "SubSurface" => MapTypeEnum.Subsurface,
            "Custom" => MapTypeEnum.Custom,
            "[1] Custom" => MapTypeEnum.CustomSecondary,
            "Opacity" => MapTypeEnum.Opacity,
            "Smoothness" => MapTypeEnum.Smoothness,
            "Emittance" => MapTypeEnum.Emittance,
            "Occlusion" => MapTypeEnum.Occlusion,
            "Specular2" => MapTypeEnum.Specular2,
            
            // Backwards-compatible names
            "Normal" => MapTypeEnum.Normals,
            "GlossNormalA" => MapTypeEnum.Smoothness,
            "Height" => MapTypeEnum.Height,
            
            _ => MapTypeEnum.Unknown,
        };
        set => MapString = value switch
        {
            MapTypeEnum.Diffuse => "Diffuse",
            MapTypeEnum.Normals => "Bumpmap",
            MapTypeEnum.Specular => "Specular",
            MapTypeEnum.Env => "Environment",
            MapTypeEnum.DetailOverlay => "Detail",
            MapTypeEnum.SecondSmoothness => "SecondSmoothness",
            MapTypeEnum.Height => "Heightmap",
            MapTypeEnum.DecalOverlay => "Decal",
            MapTypeEnum.Subsurface => "SubSurface",
            MapTypeEnum.Custom => "Custom",
            MapTypeEnum.CustomSecondary => "[1] Custom",
            MapTypeEnum.Opacity => "Opacity",
            MapTypeEnum.Smoothness => "Smoothness",
            MapTypeEnum.Emittance => "Emittance",
            MapTypeEnum.Occlusion => "Occlusion",
            MapTypeEnum.Specular2 => "Specular2",
            MapTypeEnum.Unknown => "Unknown",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

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