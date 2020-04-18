using System;
using System.Xml;
using System.Xml.Serialization;
namespace grendgine_collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Polylist : Grendgine_Collada_Geometry_Common_Fields
    {


        [XmlElement(ElementName = "vcount")]
        public Grendgine_Collada_Int_Array_String VCount;


    }
}

