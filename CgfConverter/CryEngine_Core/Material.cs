using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace CgfConverter.CryEngine_Core
{
    /// <summary>
    /// Representation of a CryEngine .mtl file
    /// </summary>
    [XmlRoot(ElementName = "Material")]
    public class Material
    {
        #region Data Structures

        /// <summary>
        /// Color used in XML serialization/deserialization
        /// </summary>
        public class Color
        {
            public Double Red;
            public Double Green;
            public Double Blue;

            /// <summary>
            /// Deserialize a string into a Color object
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Color Deserialize(String value)
            {
                if (String.IsNullOrWhiteSpace(value))
                    return null;

                String[] parts = value.Split(',');

                if (parts.Length != 3)
                    return null;

                Color buffer = new Color();

                if (!Double.TryParse(parts[0], out buffer.Red))
                    return null;

                if (!Double.TryParse(parts[1], out buffer.Green))
                    return null;

                if (!Double.TryParse(parts[2], out buffer.Blue))
                    return null;

                return buffer;
            }

            /// <summary>
            /// Serialize a Color object into a comma separated string list
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static String Serialize(Color input)
            {
                return (input == null) ? null : String.Format("{0},{1},{2}", input.Red, input.Green, input.Blue);
            }
        }

        /// <summary>
        /// The texture object
        /// </summary>
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
            public String __Map
            {
                get { return Enum.GetName(typeof(MapTypeEnum), this.Map); }
                set
                {
                    MapTypeEnum buffer = MapTypeEnum.Unknown;
                    Enum.TryParse<MapTypeEnum>(value, out buffer);
                    this.Map = buffer;
                }
            }

            /// <summary>
            /// Diffuse, Specular, Bumpmap, Environment, HeightMamp or Custom
            /// </summary>
            [XmlIgnore]
            public MapTypeEnum Map { get; set; }



            /// <summary>
            /// Location of the texture
            /// </summary>
            [XmlAttribute(AttributeName = "File")]
            public String File { get; set; }

            /// <summary>
            /// The type of the texture
            /// </summary>
            [XmlAttribute(AttributeName = "TexType")]
            [DefaultValue(TypeEnum.Default)]
            public TypeEnum TexType;

            /// <summary>
            /// The modifier to apply to the texture
            /// </summary>
            [XmlElement(ElementName = "TexMod")]
            public TextureModifier Modifier;
        }

        /// <summary>
        /// The texture modifier
        /// </summary>
        [XmlRoot(ElementName = "TexMod")]
        public class TextureModifier
        {
            [XmlAttribute(AttributeName = "TexMod_RotateType")]
            public Int32 RotateType { get; set; }

            [XmlAttribute(AttributeName = "TexMod_TexGenType")]
            public Int32 GenType { get; set; }

            [XmlAttribute(AttributeName = "TexMod_bTexGenProjected")]
            [DefaultValue(1)]
            public Int32 __Projected
            {
                get { return this.Projected ? 1 : 0; }
                set { this.Projected = value == 1; }
            }

            [XmlIgnore]
            public Boolean Projected { get; set; }

            [XmlAttribute(AttributeName = "TileU")]
            [DefaultValue(0)]
            public Double TileU { get; set; }

            [XmlAttribute(AttributeName = "TileV")]
            [DefaultValue(0)]
            public Double TileV { get; set; }

            [XmlAttribute(AttributeName = "OffsetU")]
            [DefaultValue(0)]
            public Double OffsetU { get; set; }
        }

        /// <summary>
        /// After the textures
        /// General things that apply to the material
        /// Not really needed
        /// </summary>
        [XmlRoot(ElementName = "PublicParams")]
        public class PublicParameters
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
        public String Name { get; set; }

        [XmlAttribute(AttributeName = "MtlFlags")]
        public Int32 Flags { get; set; }

        [XmlAttribute(AttributeName = "MatTemplate")]
        public String Template { get; set; }

        [XmlAttribute(AttributeName = "MatSubTemplate")]
        public String SubTemplate { get; set; }

        [XmlAttribute(AttributeName = "vertModifType")]
        public Int16 VertModifierType { get; set; }

        [XmlAttribute(AttributeName = "Shader")]
        [DefaultValue("")]
        public String Shader { get; set; }

        [XmlAttribute(AttributeName = "GenMask")]
        [DefaultValue("")]
        public String GenMask { get; set; }

        [XmlAttribute(AttributeName = "StringGenMask")]
        [DefaultValue("")]
        public String StringGenMask { get; set; }

        [XmlAttribute(AttributeName = "SurfaceType")]
        [DefaultValue(null)]
        public String SurfaceType { get; set; }

        [XmlAttribute(AttributeName = "Diffuse")]
        [DefaultValue("")]
        public String __Diffuse
        {
            get { return Color.Serialize(this.Diffuse); }
            set { this.Diffuse = Color.Deserialize(value); }
        }

        [XmlIgnore]
        public Color Diffuse { get; set; }

        [XmlAttribute(AttributeName = "Specular")]
        [DefaultValue("")]
        public String __Specular
        {
            get { return Color.Serialize(this.Specular); }
            set { this.Specular = Color.Deserialize(value); }
        }

        [XmlIgnore]
        public Color Specular { get; set; }

        [XmlAttribute(AttributeName = "Emissive")]
        [DefaultValue("")]
        public String __Emissive
        {
            get { return Color.Serialize(this.Emissive); }
            set { this.Emissive = Color.Deserialize(value); }
        }

        [XmlIgnore]
        public Color Emissive { get; set; }

        /// <summary>
        /// Value between 0 and 1 that controls opacity
        /// </summary>
        [XmlAttribute(AttributeName = "Opacity")]
        [DefaultValue(1)]
        public Double Opacity { get; set; }

        [XmlAttribute(AttributeName = "CloakAmount")]
        [DefaultValue(1)]
        public Double Cloak { get; set; }

        [XmlAttribute(AttributeName = "Shininess")]
        [DefaultValue(0)]
        public Double Shininess { get; set; }

        [XmlAttribute(AttributeName = "Glossiness")]
        [DefaultValue(0)]
        public Double Glossiness { get; set; }

        [XmlAttribute(AttributeName = "GlowAmount")]
        [DefaultValue(0)]
        public Double GlowAmount { get; set; }

        [XmlAttribute(AttributeName = "AlphaTest")]
        [DefaultValue(0)]
        public Double AlphaTest { get; set; }

        #endregion

        #region Elements

        [XmlArray(ElementName = "SubMaterials")]
        [XmlArrayItem(ElementName = "Material")]
        public Material[] SubMaterials { get; set; }

        [XmlElement(ElementName = "PublicParams")]
        public PublicParameters PublicParams { get; set; }

        // TODO: TimeOfDay Support

        [XmlArray(ElementName = "Textures")]
        [XmlArrayItem(ElementName = "Texture")]
        public Texture[] Textures { get; set; }

        #endregion

        #region Methods

        public static CryEngine_Core.Material FromFile(FileInfo materialfile)
        {
            if (!materialfile.Exists)
                return null;

            try
            {
                using (Stream fileStream = materialfile.OpenRead())
                {
                    return HoloXPLOR.DataForge.CryXmlSerializer.Deserialize<CryEngine_Core.Material>(fileStream);
                    //return HoloXPLOR.DataForge.CryXmlSerializer.Deserialize<CryEngine_Core.Material>(materialfile.FullName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0} failed deserialize - {1}", materialfile, ex.Message);
            }

            return null;
        }

        #endregion
    }
}
