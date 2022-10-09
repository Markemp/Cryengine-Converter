using System.Diagnostics;
using System.IO;
using System;
using HoloXPLOR.DataForge;

namespace CgfConverter.Materials;

public static class MaterialUtilities
{
    public static Material? FromFile(string materialfile, string? materialName)
    {
        if (!File.Exists(materialfile))
            return null;

        try
        {
            using Stream fileStream = File.OpenRead(materialfile);
            var material = CryXmlSerializer.Deserialize<Material>(fileStream);

            // For basic materials, add the material to submaterials so that all materials are consistent.
            if (material.SubMaterials is null)
            {
                material.Name = materialName;
                material.SubMaterials = new Material[1];
                material.SubMaterials[0] = material;
            }
            return material;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("{0} failed deserialize - {1}", materialfile, ex.Message);
        }

        return null;
    }

    public static Material CreateDefaultMaterial(string materialName, string diffuse = "0.5,0.5,0.5") =>
        new()
        {
            Name = materialName,
            Diffuse = diffuse,
            Specular = "1.0,1.0,1.0",
            Shininess = 0.2,
            Opacity = "1.0",
            Textures = Array.Empty<Texture>()
        };
}