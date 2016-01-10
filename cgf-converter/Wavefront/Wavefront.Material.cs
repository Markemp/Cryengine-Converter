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
        public void WriteMaterial()
        {
            if (this.CryData.Materials == null)
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
                foreach (CryEngine.Material material in this.CryData.Materials)
                {
                    file.WriteLine("newmtl {0}", material.Name);
                    file.WriteLine("Kd {0:F4} {1:F4} {2:F4}", material.Diffuse.Red, material.Diffuse.Green, material.Diffuse.Blue);
                    file.WriteLine("Ks  {0:F4} {1:F4} {2:F4}", material.Specular.Red, material.Specular.Green, material.Specular.Blue);
                    file.WriteLine("d {0:F4}", material.Opacity);
                    file.WriteLine("illum 2");  // Highlight on. This is a guess.

                    foreach (CryEngine.Material.Texture texture in material.Textures)
                    {
                        String textureFile = texture.File;

                        if (this.Args.ObjectDir != null)
                            textureFile = Path.Combine(this.Args.ObjectDir.FullName, textureFile);

                        // TODO: More filehandling here
                        
                        if (!this.Args.TiffTextures)
                            textureFile.Replace(".tif", ".dds");
                        else
                            textureFile.Replace(".dds", ".tif");

                        textureFile.Replace(@"/", @"\");
                        
                        switch (texture.Map)
                        {
                            case CryEngine.Material.Texture.MapTypeEnum.Diffuse:
                                file.WriteLine("map_Kd {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Specular:
                                file.WriteLine("map_Ks {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Bumpmap:
                            case CryEngine.Material.Texture.MapTypeEnum.Detail:
                                // <Texture Map="Detail" File="textures/unified_detail/metal/metal_scratches_a_detail.tif" />
                                file.WriteLine("map_bump {0}", textureFile);
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Heightmap:
                                // <Texture Map="Heightmap" File="objects/spaceships/ships/aegs/gladius/textures/aegs_switches_buttons_disp.tif"/>
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Decal:
                                // <Texture Map="Decal" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_alum_bare_diff.tif"/>
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.SubSurface:
                                // <Texture Map="SubSurface" File="objects/spaceships/ships/aegs/textures/interior/atlas/aegs_int_atlas_retaliator_spec.tif"/>
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Custom:
                                // <Texture Map="Custom" File="objects/spaceships/ships/aegs/textures/interior/metal/aegs_int_metal_painted_red_ddna.tif"/>
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.BlendDetail:
                                // <Texture Map="BlendDetail" File="textures/unified_detail/metal/metal_scratches-01_detail.tif">
                                break;

                            case CryEngine.Material.Texture.MapTypeEnum.Opacity:
                                // <Texture Map="Opacity" File="objects/spaceships/ships/aegs/textures/interior/blend/interior_blnd_a_diff.tif"/>
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