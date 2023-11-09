using System;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Skeleton
    {
        [XmlTextAttribute()]
        public string Value;
    }
}

