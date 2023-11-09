using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Mips_Attribute
    {

        [XmlAttribute("levels")]
        public int Levels;

        [XmlAttribute("auto_generate")]
        public bool Auto_Generate;
    }
}

