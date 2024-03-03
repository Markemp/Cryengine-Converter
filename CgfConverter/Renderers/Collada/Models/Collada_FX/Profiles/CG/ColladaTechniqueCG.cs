using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_FX.Effects;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Profiles.CG;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaTechniqueCG : ColladaEffectTechnique
{

    [XmlElement(ElementName = "annotate")]
    public ColladaAnnotate[] Annotate;

    [XmlElement(ElementName = "pass")]
    public ColladaPassCG[] Pass;
}

