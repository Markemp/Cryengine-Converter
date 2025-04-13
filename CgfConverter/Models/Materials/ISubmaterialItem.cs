namespace CgfConverter.Models.Materials;

public interface ISubMaterialItem
{
    string Name { get; set; }

    Material ResolvedMaterial { get; }
}
