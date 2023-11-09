using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada
{
    [Serializable]
    [XmlRoot(ElementName = "bump")]
    public partial class ColladaBumpMap
    {
        [XmlElement(ElementName = "texture")]
        public ColladaTexture[] Textures { get; set; }

        #region Option 2 - Convert BumpMaps to XmlElement and use the existing Data property

        /// <summary>
        /// Automatically convert Grendgine_Collada_BumpMap to XmlElements
        /// </summary>
        /// <param name="bump"></param>
        /// <returns></returns>
        public static implicit operator XmlElement(ColladaBumpMap bump)
        {
            // Can't use .ToXML helper, because XmlDocument doesn't like the <?xml> declaration at the start

            // As such, we manually serialize the object
            XmlSerializer xs = new XmlSerializer(typeof(ColladaBumpMap));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            using (var ms = new MemoryStream())
            {
                xs.Serialize(ms, bump, ns);
                ms.Seek(0, SeekOrigin.Begin);
                XmlDocument doc = new XmlDocument();
                doc.Load(ms);
                return doc.DocumentElement;
            }
        }

        /// <summary>
        /// Attempt to automatically convert XmlElement to Grendgine_Collada_BumpMap
        /// </summary>
        /// <param name="bump"></param>
        /// <returns></returns>
        public static implicit operator ColladaBumpMap(XmlElement bump)
        {
            return bump.OuterXml.FromXML<ColladaBumpMap>();
        }

        #endregion
    }
}
