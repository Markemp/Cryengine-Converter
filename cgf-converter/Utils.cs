using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter
{
    public static class Utils
    {
        public static Version GetVersion()
        {
            AssemblyVersionAttribute assemblyVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyVersionAttribute>();

            if (assemblyVersion != null)
                return new Version(assemblyVersion.Version);

            AssemblyFileVersionAttribute assemblyFileVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>();
            
            if (assemblyFileVersion != null)
                return new Version(assemblyFileVersion.Version);

            return new Version(0, 8, 0, 1);
        }
    }
}
