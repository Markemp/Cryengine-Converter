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
            // Nodraw
            // [None]
            // Illum - metal_dense
            // <PublicParams POMBumpUVBlend="1" PomDisplacement="0" SSSIndex="0" SelfShadowStrength="3" HeightBias="0.64200002" IndirectColor="0.25,0.25,0.25"/>
            // Illum - metal_damageable
            // <PublicParams BlendFalloff="1.29533" BlendLayer2SpecularColor="1,1,1" BlendLayer2Tiling="1" SSSIndex="0" BlendMaskTiling="1" BlendLayer2DiffuseColor="1,1,1" BlendLayer2Glossiness="200" BlendFactor="2.0186901" IndirectColor="0.25,0.25,0.25"/>
            // Glass - glass_thin
            // <PublicParams BumpMapTile="1" TintColor="0.6795426,0.68668544,0.59720188" CloudinessReducesGloss="0.491" TintCloudiness="0.027000001" BackLightScale="0.227" BumpScale="0.90769202" IndirectColor="0.25,0.25,0.25"/>
            // Glass - glass_dense
            // <PublicParams SSSIndex="0" IndirectColor="0.25,0.25,0.25"/>
            // Illum - [None]
            // <PublicParams BlendFalloff="2.3051701" BlendDetailGlossScale="0.30000001" BlendLayer2SpecularColor="1,1,1" BlendLayer2Tiling="1" BlendDetailDiffuseScale="0.1" DetailDiffuseScale="0" SSSIndex="0" DetailBumpScale="0.1" BlendMaskTiling="2" BlendLayer2DiffuseColor="0.67244327,0.67244327,0.67244327" BlendLayer2Glossiness="229.8" BlendDetailBumpScale="0.050000001" BlendFactor="3.8620701" DetailGlossScale="0.212" IndirectColor="0.25,0.25,0.25"/>
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
