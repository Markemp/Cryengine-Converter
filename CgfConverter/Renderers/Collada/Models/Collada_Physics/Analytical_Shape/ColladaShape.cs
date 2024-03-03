using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Geometry;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Transform;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Material;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(ElementName = "shape", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
public partial class ColladaShape
{
    [XmlElement(ElementName = "hollow")]
    public ColladaSIDBool Hollow;

    [XmlElement(ElementName = "mass")]
    public ColladaSIDFloat Mass;

    [XmlElement(ElementName = "density")]
    public ColladaSIDFloat Density;

    [XmlElement(ElementName = "physics_material")]
    public ColladaPhysicsMaterial Physics_Material;

    [XmlElement(ElementName = "instance_physics_material")]
    public ColladaInstancePhysicsMaterial Instance_Physics_Material;

    [XmlElement(ElementName = "instance_geometry")]
    public ColladaInstanceGeometry Instance_Geometry;

    [XmlElement(ElementName = "plane")]
    public ColladaPlane Plane;
    [XmlElement(ElementName = "box")]
    public ColladaBox Box;
    [XmlElement(ElementName = "sphere")]
    public ColladaSphere Sphere;
    [XmlElement(ElementName = "cylinder")]
    public ColladaCylinder Cylinder;
    [XmlElement(ElementName = "capsule")]
    public ColladaCapsule Capsule;

    [XmlElement(ElementName = "translate")]
    public ColladaTranslate[] Translate;

    [XmlElement(ElementName = "rotate")]
    public ColladaRotate[] Rotate;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;
}

