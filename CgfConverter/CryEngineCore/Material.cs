using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.CryEngineCore
{
    /// <summary>
    /// Representation of a CryEngine .mtl file
    /// </summary>
    [XmlRoot(ElementName = "Material")]
    public class Material
    {
        #region Data Structures

        internal class Color
        {
            public double Red;
            public double Green;
            public double Blue;

            /// <summary>Deserialize a string into a Color object</summary>
            public static Color Deserialize(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                string[] parts = value.Split(',');

                if (parts.Length != 3)
                    return null;

                Color buffer = new();

                if (!double.TryParse(parts[0], out buffer.Red))
                    return null;

                if (!double.TryParse(parts[1], out buffer.Green))
                    return null;

                if (!double.TryParse(parts[2], out buffer.Blue))
                    return null;

                return buffer;
            }

            /// <summary>Serialize a Color object into a comma separated string list</summary>
            public static string Serialize(Color input)
            {
                return (input == null) ? null : string.Format("{0},{1},{2}", input.Red, input.Green, input.Blue);
            }

            public override string ToString()
            {
                return $@"R: {Red}, G: {Green}, B: {Blue}";
            }
        }

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

        /// <summary>
        /// After the textures
        /// General things that apply to the material
        /// </summary>
        [XmlRoot(ElementName = "PublicParams")]
        internal class PublicParameters
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

        #endregion

        #region Attributes

        [XmlAttribute(AttributeName = "Name")]
        [DefaultValue("")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "MtlFlags")]
        public int Flags { get; set; }

        [XmlAttribute(AttributeName = "MatTemplate")]
        public string Template { get; set; }

        [XmlAttribute(AttributeName = "MatSubTemplate")]
        public string SubTemplate { get; set; }

        [XmlAttribute(AttributeName = "vertModifType")]
        public Int16 VertModifierType { get; set; }

        [XmlAttribute(AttributeName = "Shader")]
        [DefaultValue("")]
        public string Shader { get; set; }

        [XmlAttribute(AttributeName = "GenMask")]
        [DefaultValue("")]
        public string GenMask { get; set; }

        [XmlAttribute(AttributeName = "StringGenMask")]
        [DefaultValue("")]
        public string StringGenMask { get; set; }

        [XmlAttribute(AttributeName = "SurfaceType")]
        [DefaultValue(null)]
        public string SurfaceType { get; set; }

        [XmlAttribute(AttributeName = "Diffuse")]
        [DefaultValue("")]
        public string __Diffuse
        {
            get { return Color.Serialize(Diffuse); }
            set { Diffuse = Color.Deserialize(value); }
        }

        [XmlIgnore]
        internal Color Diffuse { get; set; }

        [XmlAttribute(AttributeName = "Specular")]
        [DefaultValue("")]
        public string __Specular
        {
            get { return Color.Serialize(Specular); }
            set { Specular = Color.Deserialize(value); }
        }

        [XmlIgnore]
        internal Color Specular { get; set; }

        [XmlAttribute(AttributeName = "Emissive")]
        [DefaultValue("")]
        public string __Emissive
        {
            get { return Color.Serialize(Emissive); }
            set { Emissive = Color.Deserialize(value); }
        }

        [XmlIgnore]
        internal Color Emissive { get; set; }

        /// <summary>Value between 0 and 1 that controls opacity</summary>
        [XmlAttribute(AttributeName = "Opacity")]
        [DefaultValue(1)]
        public double Opacity { get; set; }

        [XmlAttribute(AttributeName = "CloakAmount")]
        [DefaultValue(1)]
        public double Cloak { get; set; }

        [XmlAttribute(AttributeName = "Shininess")]
        [DefaultValue(0)]
        public double Shininess { get; set; }

        [XmlAttribute(AttributeName = "Glossiness")]
        [DefaultValue(0)]
        public double Glossiness { get; set; }

        [XmlAttribute(AttributeName = "GlowAmount")]
        [DefaultValue(0)]
        public double GlowAmount { get; set; }

        [XmlAttribute(AttributeName = "AlphaTest")]
        [DefaultValue(0)]
        public double AlphaTest { get; set; }

        #endregion

        #region Elements

        [XmlArray(ElementName = "SubMaterials")]
        [XmlArrayItem(ElementName = "Material")]
        public Material[] SubMaterials { get; set; }

        [XmlElement(ElementName = "PublicParams")]
        internal PublicParameters PublicParams { get; set; }

        // TODO: TimeOfDay Support

        [XmlArray(ElementName = "Textures")]
        [XmlArrayItem(ElementName = "Texture")]
        public Texture[] Textures { get; set; }

        #endregion

        public static Material FromFile(FileInfo materialfile)
        {
            try
            {
                using (Stream fileStream = materialfile.OpenRead())
                {
                    return HoloXPLOR.DataForge.CryXmlSerializer.Deserialize<Material>(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} failed deserialize - {1}", materialfile, ex.Message);
            }

            return null;
        }

        public static Material CreateDefaultMaterial(string materialName)
        {
            Material mat = new Material
            {
                Name = materialName,
                Diffuse = new Color() { Blue = 0.5, Green = 0.5, Red = 0.8 },
                Specular = new Color() { Blue = 1.0, Green = 1.0, Red = 1.0 },
                Shininess = 0.2,
                Opacity = 1.0,
                Textures = Array.Empty<Texture>()
            };

            return mat;
        }

        public override string ToString()
        {
            return $@"Name: {Name}, Diffuse: {Diffuse}";
        }
    }
}
