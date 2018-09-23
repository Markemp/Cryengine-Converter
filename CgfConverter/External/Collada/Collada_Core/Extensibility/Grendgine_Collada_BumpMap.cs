using grendgine_collada;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable]
    [XmlRoot(ElementName = "bump")]
    public partial class Grendgine_Collada_BumpMap
    {
        [XmlElement(ElementName = "texture")]
        public Grendgine_Collada_Texture[] Textures { get; set; }

        #region Option 2 - Convert BumpMaps to XmlElement and use the existing Data property

        /// <summary>
        /// Automatically convert Grendgine_Collada_BumpMap to XmlElements
        /// </summary>
        /// <param name="bump"></param>
        /// <returns></returns>
        public static implicit operator XmlElement(Grendgine_Collada_BumpMap bump)
        {
            // Can't use .ToXML helper, because XmlDocument doesn't like the <?xml> declaration at the start

            // As such, we manually serialize the object
            XmlSerializer xs = new XmlSerializer(typeof(Grendgine_Collada_BumpMap));
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
        public static implicit operator Grendgine_Collada_BumpMap (XmlElement bump)
        {
            return bump.OuterXml.FromXML<Grendgine_Collada_BumpMap>();
        }

        #endregion
    }
}
