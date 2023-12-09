using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Renderers.Collada.Collada.Types
{

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class ColladaSIDFloat
    {
        [XmlAttribute("sid")]
        public string sID;

        [XmlText()]
        public float Value;

    }
}

