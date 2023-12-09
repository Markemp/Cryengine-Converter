using System;
using System.Xml;
using System.Xml.Serialization;
using CgfConverter.Collada;
using CgfConverter.Renderers.Collada.Collada.Collada_Core.Extensibility;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Analytical_Shape;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Custom_Types;
using CgfConverter.Renderers.Collada.Collada.Collada_Physics.Physics_Material;
using CgfConverter.Renderers.Collada.Collada.Types;

namespace CgfConverter.Renderers.Collada.Collada.Collada_Physics.Technique_Common;

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ColladaTechniqueCommonRigidBody : ColladaTechniqueCommon
{
    [XmlElement(ElementName = "dynamic")]
    public ColladaSIDBool Dynamic;

    [XmlElement(ElementName = "mass")]
    public ColladaSIDFloat Mass;

    [XmlElement(ElementName = "inertia")]
    public ColladaSIDFloatArrayString Inertia;

    [XmlElement(ElementName = "mass_frame")]
    public ColladaMassFrame Mass_Frame;


    [XmlElement(ElementName = "physics_material")]
    public ColladaPhysicsMaterial Physics_Material;

    [XmlElement(ElementName = "instance_physics_material")]
    public ColladaInstancePhysicsMaterial Instance_Physics_Material;


    [XmlElement(ElementName = "shape")]
    public ColladaShape[] Shape;
}

