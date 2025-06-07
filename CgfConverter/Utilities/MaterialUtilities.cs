using System.Diagnostics;
using System.IO;
using System;
using CgfConverter.CryXmlB;
using CgfConverter.Models.Materials;
using System.Collections.Generic;
using System.Formats.Tar;

namespace CgfConverter.Utils;

public static class MaterialUtilities
{
    public static Material? FromFile(string path, string? materialName, string? objectDir = null) =>
        FromStream(new FileStream(path, FileMode.Open, FileAccess.Read), materialName, objectDir, true);

    public static Material FromStream(Stream stream, string? materialName, string? objectDir = null, bool closeAfter = false)
    {
        try
        {
            var materialsBase = CryXmlSerializer.ExtractMaterials(stream, leaveOpen: true);
            var material = CryXmlSerializer.Deserialize<Material>(stream, closeAfter);
            List<MatLayers> matLayers = [];

            // For basic materials, add the material to submaterials so that all materials are consistent.
            if (material.SubMaterials is null)
            {
                material.Name = materialName;
                material.SubMaterials = new Material[1];
                material.SubMaterials[0] = material;
            }

            var fullMats = new List<Material>();

            for (int i = 0; i < materialsBase.Count; i++)
            {
                var mat = materialsBase[i];
                if (mat is MaterialRef)
                {
                    // read the material from the reference.
                    var matfile = Path.ChangeExtension(mat.Name, ".mtl");
                    var matfileName = Path.GetFileNameWithoutExtension(matfile);
                    if (objectDir is not null)
                        matfile = Path.Combine(objectDir, matfile);
                    var refMat = CryXmlSerializer.Deserialize<Material>(new FileStream(matfile, FileMode.Open, FileAccess.Read), closeAfter);
                    refMat.Name = matfileName;
                    fullMats.Add(refMat);
                }
                else
                {
                    Material mat1 = materialsBase[i] as Material;

                    // Add MatLayers
                    fullMats.Add(mat1);
                    if (mat1.MatLayers is not null)
                    {
                        int numberOfMatLayers = mat1.MatLayers.Layers.Length;
                        mat1.SubMaterials = new Material[numberOfMatLayers];

                        int index = 0;
                        foreach (var layer in mat1.MatLayers.Layers)
                        {
                            if (objectDir is not null && layer.Path is not null)
                            {
                                var layerFileName = Path.Combine(objectDir, layer.Path);
                                try
                                {
                                    var layerMat = CryXmlSerializer.Deserialize<Material>(
                                        new FileStream(layerFileName,
                                            FileMode.Open,
                                            FileAccess.Read),
                                        closeAfter);
                                    layerMat.Name = Path.GetFileNameWithoutExtension(layer.Name);
                                    mat1.SubMaterials[index] = layerMat;
                                }
                                catch
                                {
                                    Debug.WriteLine($"Failed to deserialize layer material from {layerFileName}");
                                }
                            }
                            index++;
                        }
                    }
                }
            }

            if (fullMats is not null && fullMats.Count != 0)
                material.SubMaterials = fullMats.ToArray();

            return material;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("{0} failed deserialize - {1}", materialName, ex.Message);
            return CreateDefaultMaterial(materialName ?? "default_mat");
        }
        finally
        {
            if (closeAfter)
                stream.Close();
        }
    }

    public static Material CreateDefaultMaterial(string materialName, string diffuse = "0.5,0.5,0.5") =>
        new()
        {
            Name = materialName,
            Diffuse = diffuse,
            Specular = "1.0,1.0,1.0",
            Shininess = 0.2,
            Opacity = "1.0",
            Textures = []
        };
}
