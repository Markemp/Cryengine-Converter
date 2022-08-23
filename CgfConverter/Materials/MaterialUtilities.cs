using System.Diagnostics;
using System.IO;
using System;
using HoloXPLOR.DataForge;

namespace CgfConverter.Materials;

public static class MaterialUtilities
{
    public static Material? FromFile(string materialfile)
    {
        if (!File.Exists(materialfile))
            return null;

        try
        {
            using Stream fileStream = File.OpenRead(materialfile);
            return CryXmlSerializer.Deserialize<Material>(fileStream);
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