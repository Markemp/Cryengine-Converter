using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Math;
using System.Xml;
using System.Xml.Schema;

namespace CgfConverter
{
    public class Program
    {
        public static Int32 Main(String[] args)
        {
            ArgsHandler argsHandler = new ArgsHandler();
            Int32 result = argsHandler.ProcessArgs(args);

            try
            {
                if (result == 0)
                {
                    #region Process Input Files

                    List<CgfData> cgfData = new List<CgfData> { };

                    for (Int32 i = 0, j = argsHandler.InputFiles.Count; i < j; i++)
                    {
                        var data = new CgfData { };

                        data.ReadCgfData(argsHandler.InputFiles[i], argsHandler);

                        cgfData.Add(data);
                    }

                    #endregion

                    #region Render Output Files

                    if (argsHandler.Output_Blender == true)
                    {
                        Blender blendFile = new Blender();
                        blendFile.WriteBlend(cgfData.Last());
                    }

                    if (argsHandler.Output_Wavefront == true)
                    {
                        ObjFile objFile = new ObjFile();

                        foreach (var data in cgfData)
                            objFile.WriteObjFile(data); // cgfData.Last());
                    }

                    if (argsHandler.Output_Collada == true)
                    {
                        COLLADA daeFile = new COLLADA();
                        daeFile.WriteCollada(cgfData.Last());
                    }

                    #endregion
                }
            }
            catch (Exception)
            {
                if (argsHandler.Throw)
                    throw;
            }

            return result;
        }
    }
}
