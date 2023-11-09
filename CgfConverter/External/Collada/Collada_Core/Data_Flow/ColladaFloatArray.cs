using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaFloatArray : ColladaFloatArrayString
    {
        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("count")]
        public int Count;

        [XmlAttribute("digits")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "6")]
        public int Digits;

        [XmlAttribute("magnitude")]
        [System.ComponentModel.DefaultValueAttribute(typeof(int), "38")]
        public int Magnitude;

    }
}

