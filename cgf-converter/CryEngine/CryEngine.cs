using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public partial class CryEngine
    {
        #region Constructors

        public CryEngine(ArgsHandler argsHandler)
        {
            this.Asset = new CryEngine.Model(argsHandler);

            // TODO: Check for mFile

            foreach (CryEngine.Model.ChunkMtlName mtlChunk in this.Asset.CgfChunks.Where(c => c.chunkType == ChunkType.MtlName))
            {
                // Don't process child materials for now
                if (mtlChunk.version == 0x800 && !(mtlChunk.MatType == 0x01 || mtlChunk.MatType == 0x10))
                    continue;

                Console.WriteLine("Found material {0}", mtlChunk.Name);

                // First try relative to file being processed
                FileInfo materialFile = new FileInfo(Path.Combine(argsHandler.InputFiles.First().Directory.FullName, mtlChunk.Name));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try relative to the ObjectDir
                if (!materialFile.Exists)
                    materialFile = new FileInfo(Path.Combine(argsHandler.ObjectDir.FullName, mtlChunk.Name));
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // Then try just the original file name
                if (!materialFile.Exists)
                    materialFile = new FileInfo(argsHandler.InputFiles.First().FullName);
                if (materialFile.Extension != "mtl")
                    materialFile = new FileInfo(Path.ChangeExtension(materialFile.FullName, "mtl"));

                // TODO: Try more paths

                CryEngine.Material material = CryEngine.Material.FromFile(materialFile);

                if (material != null)
                {
                    this.Materials = this.FlattenMaterials(material).Skip(1).ToArray();
                    // UInt32 i = 0;
                    // this.MaterialMap = this.Materials.Skip(1).ToArray(); // .ToDictionary(k => ++i, v => v);

                    // Early return - we have the material map
                    return;
                }
            }

            this.Materials = new Material[] { };
        }

        #endregion

        #region Properties

        public CryEngine.Model Asset { get; private set; }
        public CryEngine.Material[] Materials { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Flatten all child materials into a one dimensional list
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private IEnumerable<CryEngine.Material> FlattenMaterials(CryEngine.Material material)
        {
            if (material != null)
            {
                yield return material;

                if (material.SubMaterials != null)
                    foreach (var subMaterial in material.SubMaterials.SelectMany(m => this.FlattenMaterials(m)))
                        yield return subMaterial;
            }
        }

        #endregion
    }
}
