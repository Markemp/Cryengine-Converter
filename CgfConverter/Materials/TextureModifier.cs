﻿using System.ComponentModel;
using System.Xml.Serialization;

namespace CgfConverter.Materials;

/// <summary>The texture modifier</summary>
[XmlRoot(ElementName = "TexMod")]
public class TextureModifier
{
    [XmlAttribute(AttributeName = "TileU")]
    [DefaultValue(0)]
    public float TileU { get; set; }

    [XmlAttribute(AttributeName = "TileV")]
    [DefaultValue(0)]
    public float TileV { get; set; }

    [XmlAttribute(AttributeName = "OffsetU")]
    [DefaultValue(0)]
    public float OffsetU { get; set; }

    [XmlAttribute(AttributeName = "OffsetV")]
    [DefaultValue(0)]
    public float OffsetV { get; set; }

    [XmlAttribute(AttributeName = "TexMod_bTexGenProjected")]
    [DefaultValue(1)]
    public int __Projected
    {
        get { return this.Projected ? 1 : 0; }
        set { Projected = value == 1; }
    }

    [XmlIgnore]
    public bool Projected { get; set; }

    [XmlAttribute(AttributeName = "TexMod_UOscillatorType")]
    [DefaultValue(ETexModMoveType.NoChange)]    
    public ETexModMoveType UOscillatorType;
    
    [XmlAttribute(AttributeName = "TexMod_VOscillatorType")]
    [DefaultValue(ETexModMoveType.NoChange)]
    public ETexModMoveType VOscillatorType;
    
    [XmlAttribute(AttributeName = "TexMod_RotateType")]
    [DefaultValue(ETexModRotateType.NoChange)]
    public ETexModRotateType RotateType { get; set; } 

    [XmlAttribute(AttributeName = "TexMod_TexGenType")]
    [DefaultValue(ETexGenType.Stream)]
    public ETexGenType GenType { get; set; }

    [XmlAttribute(AttributeName = "RotateU")]
    [DefaultValue(0)]
    public float RotateU { get; set; }

    [XmlAttribute(AttributeName = "RotateV")]
    [DefaultValue(0)]
    public float RotateV { get; set; }

    [XmlAttribute(AttributeName = "RotateW")]
    [DefaultValue(0)]
    public float RotateW { get; set; }

    [XmlAttribute(AttributeName = "TexMod_URotateRate")]
    public float URotateRate { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VRotateRate")]
    public float VRotateRate { get; set; }

    [XmlAttribute(AttributeName = "TexMod_WRotateRate")]
    public float WRotateRate { get; set; }

    [XmlAttribute(AttributeName = "TexMod_URotatePhase")]
    public float URotatePhase { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VRotatePhase")]
    public float VRotatePhase { get; set; }

    [XmlAttribute(AttributeName = "TexMod_WRotatePhase")]
    public float WRotatePhase { get; set; }

    [XmlAttribute(AttributeName = "TexMod_URotateAmplitude")]
    public float URotateAmplitude { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VRotateAmplitude")]
    public float VRotateAmplitude { get; set; }

    [XmlAttribute(AttributeName = "TexMod_WRotateAmplitude")]
    public float WRotateAmplitude { get; set; }

    [XmlAttribute(AttributeName = "TexMod_URotateCenter")]
    public float URotateCenter { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VRotateCenter")]
    public float VRotateCenter { get; set; }

    [XmlAttribute(AttributeName = "TexMod_WRotateCenter")]
    public float WRotateCenter { get; set; }

    [XmlAttribute(AttributeName = "TexMod_UOscillatorRate")]
    public float UOscillatorRate { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VOscillatorRate")]
    public float VOscillatorRate { get; set; }

    [XmlAttribute(AttributeName = "TexMod_UOscillatorPhase")]
    public float UOscillatorPhase { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VOscillatorPhase")]
    public float VOscillatorPhase { get; set; }

    [XmlAttribute(AttributeName = "TexMod_UOscillatorAmplitude")]
    public float UOscillatorAmplitude { get; set; }

    [XmlAttribute(AttributeName = "TexMod_VOscillatorAmplitude")]
    public float VOscillatorAmplitude { get; set; }
}
