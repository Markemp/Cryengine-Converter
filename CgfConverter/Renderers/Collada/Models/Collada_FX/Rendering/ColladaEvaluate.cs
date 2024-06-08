using System;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Renderers.Collada.Collada.Collada_FX.Rendering;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "evaluate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaEffectTechniqueEvaluate
{

    [XmlElement(ElementName = "color_target")]
    public ColladaColorTarget Color_Target;

    [XmlElement(ElementName = "depth_target")]
    public ColladaDepthTarget Depth_Target;

    [XmlElement(ElementName = "stencil_target")]
    public ColladaStencilTarget Stencil_Target;

    [XmlElement(ElementName = "color_clear")]
    public ColladaColorClear Color_Clear;

    [XmlElement(ElementName = "depth_clear")]
    public ColladaDepthClear Depth_Clear;

    [XmlElement(ElementName = "stencil_clear")]
    public ColladaStencilClear Stencil_Clear;

    [XmlElement(ElementName = "draw")]
    public ColladaDraw Draw;


}

