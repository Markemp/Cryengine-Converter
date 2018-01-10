using System;
using HoloXPLOR.DataForge;
using CgfConverter;
using CgfConverter.Models;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Material_Library_Creator
{
    public class Program
    {
        static void Main(string[] args)
        {
            MaterialLibrary matLibrary = new MaterialLibrary()
            {
                BaseDirectory = Environment.CurrentDirectory,
            };

            //string fileName = args[0];
            //string fileName = @"d:\depot\sc\materials\test\ball_02_mat.mtl";
            //string materialFileName = @"d:\depot\sc-3.0\data\materials\planets\planet_SSS.mtl";
            //string prefabFile = @"d:\depot\mwo\prefabs\mechlab.xml";
            //string prefabFileSC = @"d:\depot\sc\prefabs\Outlawstation.xml";
            Console.WriteLine("Base Directory is " + Environment.CurrentDirectory);
            string[] materialFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.mtl", SearchOption.AllDirectories);

            //FileInfo materialFile = new FileInfo(materialFileName);
            try
            {
                foreach (string file in materialFiles)
                {
                    //Material material = Material.FromFile(new FileInfo(file));
                    CgfConverter.CryEngine_Core.Material materials = CgfConverter.CryEngine_Core.Material.FromFile(new FileInfo(file));
                    // All the materials in this file are in the materials variable.  For each material in here, create a materiallibraryitem.
                    if (materials.Name != null)
                    {
                        // Simple material format, with no submats.  This is the only material.
                        MaterialLibraryItem item = new MaterialLibraryItem()
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
                Console.WriteLine("*** Exception converting XML: ", ex.Message);
            }
            WriteMaterialLibrary(matLibrary);
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        public static void WriteMaterialLibrary(MaterialLibrary library)
        {
            using (StreamWriter file = new StreamWriter(Environment.CurrentDirectory + @"material_library.json"))
            //using (StreamWriter file = new StreamWriter(@"d:\depot\mwo\material_library.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, library);
            }

        }
    }
}
