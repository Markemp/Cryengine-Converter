using System.Diagnostics;
using System.IO;
using System;

namespace CgfConverter.Materials;

internal static class MaterialUtilities
{
    public static Material FromFile(FileInfo materialfile)
    {
        if (!materialfile.Exists)
            return null;

        try
        {
            using Stream fileStream = materialfile.OpenRead();
            return HoloXPLOR.DataForge.CryXmlSerializer.Deserialize<Material>(fileStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("{0} failed deserialize - {1}", materialfile, ex.Message);
        }

        return null;
    }
}