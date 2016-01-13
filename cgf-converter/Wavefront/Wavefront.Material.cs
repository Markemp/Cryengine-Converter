using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CgfConverter
{
    public partial class Wavefront
    {
        public void WriteMaterial(CryEngine cryEngine)
        {
            if (cryEngine.Materials == null)
            {
                Console.WriteLine("No materials loaded");
                return;
            }

            if (!this.OutputFile_Material.Directory.Exists)
                this.OutputFile_Material.Directory.Create();

            using (StreamWriter file = new StreamWriter(this.OutputFile_Material.FullName))
            {
                file.WriteLine("# Material file output from cgf-converter.exe version {0}", Utils.GetVersion());
                file.WriteLine("#");
                foreach (CryEngine.Material material in cryEngine.Materials)
                {
#if DUMP_JSON
                    File.WriteAllText(String.Format("_material-{0}.json", material.Name.Replace(@"/", "").Replace(@"\", "")), material.ToJSON());
#endif
       
                    file.WriteLine("newmtl {0}", material.Name);
                    if (material.Diffuse != null)
                    {
                        file.WriteLine("Ka {0:F6} {1:F6} {2:F6}", material.Diffuse.Red, material.Diffuse.Green, material.Diffuse.Blue);    // Ambient
                        file.WriteLine("Kd {0:F6} {1:F6} {2:F6}", material.Diffuse.Red, material.Diffuse.Green, material.Diffuse.Blue);    // Diffuse
                    }
                    else
                    {
                        Console.WriteLine("Skipping Diffuse for {0}", material.Name);
                    }
                    if (material.Specular != null)
                    {
                        file.WriteLine("Ks {0:F6} {1:F6} {2:F6}", material.Specular.Red, material.Specular.Green, material.Specular.Blue); // Specular
                        file.WriteLine("Ns {0:F6}", material.Shininess / 255D);                                                            // Specular Exponent
                    }
                    else
                    {
                        Console.WriteLine("Skipping Specular for {0}", material.Name);
                    }
                    file.WriteLine("d {0:F6}", material.Opacity);                                                                          // Dissolve

                    file.WriteLine("illum 2");  // Highlight on. This is a guess.

                    // Phong materials

                    // 0. Color on and Ambient off
                    // 1. Color on and Ambient on
                    // 2. Highlight on
                    // 3. Reflection on and Ray trace on
                    // 4. Transparency: Glass on, Reflection: Ray trace on
                    // 5. Reflection: Fresnel on and Ray trace on
                    // 6. Transparency: Refraction on, Reflection: Fresnel off and Ray trace on
                    // 7. Transparency: Refraction on, Reflection: Fresnel on and Ray trace on
                    // 8. Reflection on and Ray trace off
                    // 9. Transparency: Glass on, Reflection: Ray trace off
                    // 10. Casts shadows onto invisible surfaces

                    foreach (CryEngine.Material.Texture texture in material.Textures)
                    {
                        String textureFile = texture.File;

                        if (this.Args.DataDir != null)
                            textureFile = Path.Combine(this.Args.DataDir, textureFile);

                        // TODO: More filehandling here
                        
                        if (!this.Args.TiffTextures)
                            textureFile = textureFile.Replace(".tif", ".dds");
                        else
                            textureFile = textureFile.Replace(".dds", ".tif");

                        textureFile = textureFile.Replace(@"/", @"\");
                        
                        switch (texture.Map)
                        {
                            case CryEngine.Material.Texture.MapTypeEnum.Diffuse:
                                file.WriteLine("map_Kd {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Specular:
                                file.WriteLine("map_Ks {0}", textureFile);
                                file.WriteLine("map_Ns {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Bumpmap:
                            case CryEngine.Material.Texture.MapTypeEnum.Detail:
                                // <Texture Map="Detail" File="textures/unified_detail/metal/metal_scratches_a_detail.tif" />
                                file.WriteLine("map_bump {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Heightmap:
                                // <Texture Map="Heightmap" File="objects/spaceships/ships/aegs/gladius/textures/aegs_switches_buttons_disp.tif"/>
                                file.WriteLine("disp {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Decal:
                                // <Texture Map="Decal" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_alum_bare_diff.tif"/>
                                file.WriteLine("decal {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.SubSurface:
                                // <Texture Map="SubSurface" File="objects/spaceships/ships/aegs/textures/interior/atlas/aegs_int_atlas_retaliator_spec.tif"/>
                                file.WriteLine("map_Ns {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Custom:
                                // <Texture Map="Custom" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_painted_red_ddna.tif"/>
                                // file.WriteLine("decal {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.BlendDetail:
                                // <Texture Map="BlendDetail" File="textures/unified_detail/metal/metal_scratches-01_detail.tif">
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Opacity:
                                // <Texture Map="Opacity" File="objects/spaceships/ships/aegs/textures/interior/blend/interior_blnd_a_diff.tif"/>
                                file.WriteLine("map_d {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Environment:
                                // <Texture Map="Environment" File="nearest_cubemap" TexType="7"/>
                                break;

                            default:
                                break;
                        }
                    }
                    file.WriteLine();
                }
            }
        }
    }
}