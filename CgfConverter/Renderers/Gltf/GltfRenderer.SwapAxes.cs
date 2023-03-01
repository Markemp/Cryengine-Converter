using System.Numerics;

namespace CgfConverter.Renderers.Gltf;

public partial class GltfRenderer
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
     */
    
    private static Vector3 SwapAxes(Vector3 val) => new(val.X, val.Z, val.Y);

    private static Quaternion SwapAxes(Quaternion val) => new(val.X, val.Z, val.Y, -val.W);

    private static Matrix4x4 SwapAxes(Matrix4x4 val) => new(
        val.M11, val.M13, val.M12, val.M14,
        val.M31, val.M33, val.M32, val.M34,
        val.M21, val.M23, val.M22, val.M24,
        val.M41, val.M43, val.M42, val.M44);
}