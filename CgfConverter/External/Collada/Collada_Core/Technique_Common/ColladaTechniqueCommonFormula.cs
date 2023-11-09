using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada;

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

