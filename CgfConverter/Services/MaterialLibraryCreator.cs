using System;
using System.IO;
using CgfConverter.Models;

namespace CgfConverter.Services;

public class MaterialLibraryCreator
{
    public void CreateMaterialLibrary()
    {
        MaterialLibrary matLibrary = new MaterialLibrary()
        {
            BaseDirectory = Environment.CurrentDirectory,
        };

        Utils.Log(LogLevelEnum.Info, "Base Directory is " + Environment.CurrentDirectory);
        string[] materialFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.mtl", SearchOption.AllDirectories);

        //FileInfo materialFile = new FileInfo(materialFileName);
        try
        {
            foreach (string file in materialFiles)
            {
                Utils.Log(LogLevelEnum.Info, "Processing " + file);
                //Material material = Material.FromFile(new FileInfo(file));
                CgfConverter.CryEngineCore.Material materials = CgfConverter.CryEngineCore.Material.FromFile(new FileInfo(file));
                // All the materials in this file are in the materials variable.  For each material in here, create a materiallibraryitem.
                if (materials.Name != null)
                {
                    // Simple material format, with no submats.  This is the only material.
                    var item = new MaterialLibraryItem()
                    {
                        Material = materials,
                        Guid = new Guid(),
                        MaterialFileSource = file
                    };
                    matLibrary.MaterialLibraryItems.Add(item);
                }
                else
                {
                    foreach (var material in materials.SubMaterials)
                    {
                        if (material.Name != null)          // Don't get the root node
                        {
                            MaterialLibraryItem item = new MaterialLibraryItem()
                            {
                                Material = material,
                                Guid = Guid.NewGuid(),
                                MaterialFileSource = file
                            };
                            matLibrary.MaterialLibraryItems.Add(item);
                        }
                    }
                }
            }

            //PrefabsLibrary prefabLibrary = PrefabsLibrary.FromFile(new FileInfo(prefabFileSC));
        }
        catch (Exception ex)
        {
            Utils.Log(LogLevelEnum.Critical, "*** Exception converting XML: ", ex.Message);
        }
        WriteMaterialLibrary(matLibrary);
        Console.WriteLine("Press any key to close.");
        Console.ReadKey();
    }

    public static void WriteMaterialLibrary(MaterialLibrary library)
    {
        using var file = new StreamWriter(Environment.CurrentDirectory + @"material_library.json");
    }
}
