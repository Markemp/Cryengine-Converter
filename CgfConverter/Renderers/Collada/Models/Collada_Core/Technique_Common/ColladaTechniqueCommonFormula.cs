using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
namespace CgfConverter.Renderers.Collada.Collada.Collada_Core.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonFormula : ColladaTechniqueCommon
{
    /// <summary>
    /// Need to determine the type and value of the Object(s)
    /// </summary>
    [XmlAnyElement]
    public XmlElement[] Data;

}

