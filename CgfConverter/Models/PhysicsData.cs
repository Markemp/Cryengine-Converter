using System.Numerics;
using CgfConverter.Models.Structs;

namespace CgfConverter;

public sealed record PhysicsData
{
    // Collision or hitbox info.  Part of the MeshPhysicsData chunk
    public int Unknown4 { get; set; }
    public int Unknown5 { get; set; }
    public float[] Unknown6 { get; set; }  // array length 3, Inertia?
    public Quaternion Rot { get; set; }  // Most definitely a quaternion. Probably describes rotation of the physics object.
    public Vector3 Center { get; set; }  // Center, or position. Probably describes translation of the physics object. Often corresponds to the center of the mesh data as described in the submesh chunk.
    public float Unknown10 { get; set; } // Mass?
    public int Unknown11 { get; set; }
    public int Unknown12 { get; set; }
    public float Unknown13 { get; set; }
    public float Unknown14 { get; set; }
    public PhysicsPrimitiveType PrimitiveType { get; set; }
    public PhysicsCube Cube { get; set; }  // Primitive Type 0
    public PhysicsPolyhedron PolyHedron { get; set; }  // Primitive Type 1
    public PhysicsCylinder Cylinder { get; set; } // Primitive Type 5
    public PhysicsShape6 UnknownShape6 { get; set; }  // Primitive Type 6
}
