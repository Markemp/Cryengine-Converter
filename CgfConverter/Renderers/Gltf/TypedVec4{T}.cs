namespace CgfConverter.Renderers.Gltf;

public struct TypedVec4<T> where T : unmanaged
{
    public T V1 = default;
    public T V2 = default;
    public T V3 = default;
    public T V4 = default;

    public TypedVec4()
    {
    }

    public TypedVec4(T v1, T v2, T v3, T v4)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        V4 = v4;
    }
}