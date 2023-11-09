using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Size_2D
    {

        [XmlAttribute("width")]
        public int Width;

        [XmlAttribute("height")]
        public int Height;
    }
}

