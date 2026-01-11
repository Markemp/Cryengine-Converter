using System.Collections.Generic;
using System.Numerics;

namespace CgfConverter.Renderers.Gltf;

public partial class BaseGltfRenderer
{
    /*
     * https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#coordinate-system-and-units
     * glTF uses a right-handed coordinate system.
     * glTF defines +Y as up, +Z as forward, and -X as right.
     * the front of a glTF asset faces +Z.
     *
     * https://docs.cryengine.com/display/CEPROG/Math
     * CRYENGINE uses a right-handed coordinate system
     * where positive X-axis points to the right, positive Y-axis away from the viewer and positive Z-axis points up.
     * In the context of characters this means that positive X is right, positive Y is forward, and positive Z is up.
     *
     * Blender glTF import comment (blender_gltf.py):
     *   # glTF: right = +X, forward = -Z, up = +Y
     *   # glTF after Yup2Zup: right = +X, forward = +Y, up = +Z
     *   # Blender: right = +X, forward = -Z, up = +Y
     *
     * To match USD export in Blender (where models face +Y after import):
     * We map CryEngine +Y (forward) → glTF -Z, so after Blender's Y-to-Z conversion,
     * the model faces Blender +Y (matching our USD export which uses upAxis="Z").
     *
     *         glTF cryengine
     * Right     +X        +X   (same, no flip needed for consistency with Blender)
     * Up        +Y        +Z
     * Forward   -Z        +Y   (CryEngine +Y → glTF -Z for Blender compatibility)
     */

    // Original mapping (model faces Blender -Y after import):
    // protected static Vector3 SwapAxesForPosition(Vector3 val) => new(-val.X, val.Z, val.Y);

    // Corrected mapping (model faces Blender +Y after import, matching USD):
    // CryEngine (X, Y, Z) → glTF (X, Z, -Y) so that:
    //   - CryEngine +X (right) → glTF +X
    //   - CryEngine +Z (up) → glTF +Y (up)
    //   - CryEngine +Y (forward) → glTF -Z → Blender +Y (forward)
    protected static Vector3 SwapAxesForPosition(Vector3 val) => new(val.X, val.Z, -val.Y);

    protected static Vector3 SwapAxesForScale(Vector3 val) => new(val.X, val.Z, val.Y);

    // Tangent vector transforms like position: (x, y, z, w) → (x, z, -y, w)
    protected static Vector4 SwapAxesForTangent(Vector4 val) => new(val.X, val.Z, -val.Y, val.W);

    // Quaternion rotation axis transforms like a vector: (qx, qy, qz, qw) → (qx, qz, -qy, qw)
    protected static Quaternion SwapAxesForAnimations(Quaternion val) => new(val.X, val.Z, -val.Y, val.W);

    // Layout quaternion uses same transformation as animations
    protected static Quaternion SwapAxesForLayout(Quaternion val) => new(val.X, val.Z, -val.Y, val.W);

    // M':   swapped matrix
    // T:    swap matrix for (X, Y, Z) → (X, Z, -Y):
    //       | 1  0  0  0 |
    //       | 0  0  1  0 |
    //       | 0 -1  0  0 |
    //       | 0  0  0  1 |
    // M' = T @ M @ T^-1
    protected static Matrix4x4 SwapAxes(Matrix4x4 val) =>
        new(
            val.M11, val.M13, -val.M12, val.M14,
            val.M31, val.M33, -val.M32, val.M34,
            -val.M21, -val.M23, val.M22, -val.M24,
            val.M41, val.M43, -val.M42, val.M44);

    /// <summary>
    /// Converts a Matrix4x4 to glTF's column-major format (16-element list).
    /// glTF expects matrices in column-major order: [m00, m10, m20, m30, m01, m11, m21, m31, ...]
    /// </summary>
    protected static List<float> MatrixToGltfList(Matrix4x4 m) =>
    [
        m.M11, m.M21, m.M31, m.M41,  // Column 0
        m.M12, m.M22, m.M32, m.M42,  // Column 1
        m.M13, m.M23, m.M33, m.M43,  // Column 2
        m.M14, m.M24, m.M34, m.M44   // Column 3
    ];



    /*
    // Use these if you need to confirm directions
    protected static Vector3 SwapAxesForPosition(Vector3 x) => x;
    protected static Vector3 SwapAxesForScale(Vector3 x) => x;
    protected static Quaternion SwapAxesForLayout(Quaternion x) => x;
    protected static Quaternion SwapAxesForAnimations(Quaternion x) => x;
    protected static Matrix4x4 SwapAxes(Matrix4x4 x) => x;
    */
}
