using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Custom_Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;

[Serializable]
[XmlRoot(ElementName = "bump")]
public partial class ColladaBumpMap
{
    [XmlElement(ElementName = "texture")]
    public ColladaTexture[] Textures { get; set; }

    #region Option 2 - Convert BumpMaps to XmlElement and use the existing Data property

    /// <summary>
    /// Automatically convert ColladaBumpMap to XmlElements
    /// </summary>
    /// <param name="bump"></param>
    /// <returns></returns>
    public static implicit operator XmlElement(ColladaBumpMap bump)
    {
        // Can't use .ToXML helper, because XmlDocument doesn't like the <?xml> declaration at the start
        // As such, we manually serialize the object
        XmlSerializer xs = new(typeof(ColladaBumpMap));
        XmlSerializerNamespaces ns = new();
        ns.Add("", "");
        using var ms = new MemoryStream();
        xs.Serialize(ms, bump, ns);
        ms.Seek(0, SeekOrigin.Begin);
        XmlDocument doc = new XmlDocument();
        doc.Load(ms);
        return doc.DocumentElement;
    }

    /// <summary>
    /// Attempt to automatically convert XmlElement to ColladaBumpMap
    /// </summary>
    /// <param name="bump"></param>
    /// <returns></returns>
    public static implicit operator ColladaBumpMap(XmlElement bump)
    {
        return bump.OuterXml.FromXML<ColladaBumpMap>();
    }

    #endregion
}
