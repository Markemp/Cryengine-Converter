using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace CgfConverter.Models;

/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
[XmlRoot(ElementName = "PublicParams")]
public class PublicParams
{
    [XmlAttribute(AttributeName = "FresnelPower")]
    public string FresnelPower { get; set; }
    [XmlAttribute(AttributeName = "GlossFromDiffuseContrast")]
    public string GlossFromDiffuseContrast { get; set; }
    [XmlAttribute(AttributeName = "FresnelScale")]
    public string FresnelScale { get; set; }
    [XmlAttribute(AttributeName = "GlossFromDiffuseOffset")]
    public string GlossFromDiffuseOffset { get; set; }
    [XmlAttribute(AttributeName = "FresnelBias")]
    public string FresnelBias { get; set; }
    [XmlAttribute(AttributeName = "GlossFromDiffuseAmount")]
    public string GlossFromDiffuseAmount { get; set; }
    [XmlAttribute(AttributeName = "GlossFromDiffuseBrightness")]
    public string GlossFromDiffuseBrightness { get; set; }
    [XmlAttribute(AttributeName = "IndirectColor")]
    public string IndirectColor { get; set; }
    [XmlAttribute(AttributeName = "SpecMapChannelB")]
    public string SpecMapChannelB { get; set; }
    [XmlAttribute(AttributeName = "SpecMapChannelR")]
    public string SpecMapChannelR { get; set; }
    [XmlAttribute(AttributeName = "GlossMapChannelB")]
    public string GlossMapChannelB { get; set; }
    [XmlAttribute(AttributeName = "SpecMapChannelG")]
    public string SpecMapChannelG { get; set; }
    [XmlAttribute(AttributeName = "DirtTint")]
    public string DirtTint { get; set; }
    [XmlAttribute(AttributeName = "DirtGlossFactor")]
    public string DirtGlossFactor { get; set; }
    [XmlAttribute(AttributeName = "DirtTiling")]
    public string DirtTiling { get; set; }
    [XmlAttribute(AttributeName = "DirtStrength")]
    public string DirtStrength { get; set; }
    [XmlAttribute(AttributeName = "DirtMapAlphaInfluence")]
    public string DirtMapAlphaInfluence { get; set; }
    [XmlAttribute(AttributeName = "DetailBumpTillingU")]
    public string DetailBumpTillingU { get; set; }
    [XmlAttribute(AttributeName = "DetailDiffuseScale")]
    public string DetailDiffuseScale { get; set; }
    [XmlAttribute(AttributeName = "DetailBumpScale")]
    public string DetailBumpScale { get; set; }
    [XmlAttribute(AttributeName = "DetailGlossScale")]
    public string DetailGlossScale { get; set; }
}

[XmlRoot(ElementName = "Material")]
public class Material
{
    [XmlElement(ElementName = "PublicParams")]
    public PublicParams PublicParams { get; set; }
    [XmlAttribute(AttributeName = "Name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "MtlFlags")]
    public string MtlFlags { get; set; }
    [XmlAttribute(AttributeName = "Shader")]
    public string Shader { get; set; }
    [XmlAttribute(AttributeName = "GenMask")]
    public string GenMask { get; set; }
    [XmlAttribute(AttributeName = "StringGenMask")]
    public string StringGenMask { get; set; }
    [XmlAttribute(AttributeName = "SurfaceType")]
    public string SurfaceType { get; set; }
    [XmlAttribute(AttributeName = "MatTemplate")]
    public string MatTemplate { get; set; }
    [XmlAttribute(AttributeName = "Diffuse")]
    public string Diffuse { get; set; }
    [XmlAttribute(AttributeName = "Specular")]
    public string Specular { get; set; }
    [XmlAttribute(AttributeName = "Emissive")]
    public string Emissive { get; set; }
    [XmlAttribute(AttributeName = "Shininess")]
    public string Shininess { get; set; }
    [XmlAttribute(AttributeName = "Opacity")]
    public string Opacity { get; set; }
    [XmlElement(ElementName = "Textures")]
    public Textures Textures { get; set; }
    [XmlAttribute(AttributeName = "AlphaTest")]
    public string AlphaTest { get; set; }

    public static Material FromFile(FileInfo materialfile)
    {
        if (!materialfile.Exists)
            return null;

        try
        {
            using Stream fileStream = materialfile.OpenRead();
            return HoloXPLOR.DataForge.CryXmlSerializer.Deserialize<Material>(fileStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("{0} failed deserialize - {1}", materialfile, ex.Message);
        }

        return null;
    }
}

[XmlRoot(ElementName = "Texture")]
public class Texture
{
    [XmlAttribute(AttributeName = "Map")]
    public string Map { get; set; }
    [XmlAttribute(AttributeName = "File")]
    public string File { get; set; }
}

[XmlRoot(ElementName = "Textures")]
public class Textures
{
    [XmlElement(ElementName = "Texture")]
    public List<Texture> Texture { get; set; }
}

[XmlRoot(ElementName = "SubMaterials")]
public class SubMaterials
{
    [XmlElement(ElementName = "Material")]
    public List<Material> Material { get; set; }
}

[XmlRoot(ElementName = "xml")]
public class Xml
{
    [XmlElement(ElementName = "Material")]
    public Material Material { get; set; }
}


