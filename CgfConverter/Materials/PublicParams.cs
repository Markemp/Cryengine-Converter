using System.Xml.Serialization;

namespace CgfConverter.Materials;

[XmlRoot(ElementName = "PublicParams")]
public record PublicParams(
    [XmlAttribute(AttributeName = "FresnelPower")]
    string FresnelPower,
    [XmlAttribute(AttributeName = "GlossFromDiffuseContrast")]
    string GlossFromDiffuseContrast,
    [XmlAttribute(AttributeName = "FresnelScale")]
    string FresnelScale,
    [XmlAttribute(AttributeName = "GlossFromDiffuseOffset")]
    string GlossFromDiffuseOffset,
    [XmlAttribute(AttributeName = "FresnelBias")]
    string FresnelBias,
    [XmlAttribute(AttributeName = "GlossFromDiffuseAmount")]
    string GlossFromDiffuseAmount,
    [XmlAttribute(AttributeName = "GlossFromDiffuseBrightness")]
    string GlossFromDiffuseBrightness,
    [XmlAttribute(AttributeName = "IndirectColor")]
    string IndirectColor,
    [XmlAttribute(AttributeName = "SpecMapChannelB")]
    string SpecMapChannelB,
    [XmlAttribute(AttributeName = "SpecMapChannelR")]
    string SpecMapChannelR,
    [XmlAttribute(AttributeName = "GlossMapChannelB")]
    string GlossMapChannelB,
    [XmlAttribute(AttributeName = "SpecMapChannelG")]
    string SpecMapChannelG,
    [XmlAttribute(AttributeName = "DirtTint")]
    string DirtTint,
    [XmlAttribute(AttributeName = "DirtGlossFactor")]
    string DirtGlossFactor,
    [XmlAttribute(AttributeName = "DirtTiling")]
    string DirtTiling,
    [XmlAttribute(AttributeName = "DirtStrength")]
    string DirtStrength,
    [XmlAttribute(AttributeName = "DirtMapAlphaInfluence")]
    string DirtMapAlphaInfluence,
    [XmlAttribute(AttributeName = "DetailBumpTillingU")]
    string DetailBumpTillingU,
    [XmlAttribute(AttributeName = "DetailDiffuseScale")]
    string DetailDiffuseScale,
    [XmlAttribute(AttributeName = "DetailBumpScale")]
    string DetailBumpScale,
    [XmlAttribute(AttributeName = "DetailGlossScale")]
    string DetailGlossScale
);