using System;
using System.Xml;
using System.Xml.Serialization;
namespace CgfConverter.Collada
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_GLES : ColladaEffectTechnique
    {

        [XmlElement(ElementName = "annotate")]
        public ColladaAnnotate[] Annotate;

        [XmlElement(ElementName = "pass")]
        public Grendgine_Collada_Pass_GLES[] Pass;
    }
}

