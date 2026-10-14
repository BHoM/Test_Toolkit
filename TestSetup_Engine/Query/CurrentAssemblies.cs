using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Setup
{
    public static partial class Query
    {
        public static List<Assembly> CurrentAssemblies()
        {
            List<Assembly> assembliesToTest = Setup.Query.InputParametersAssemblies();
            if (assembliesToTest == null)
            {
                assembliesToTest = GetProjectFilesAsAssemblies();
            }
            return assembliesToTest;
        }

        private static List<Assembly> GetProjectFilesAsAssemblies()
        {
            List<string> files = Setup.Query.GetFiles(System.IO.Path.Combine(Setup.Query.CurrentRepoFolder()), "*.csproj", true).ToList();
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                string assemblyPath = System.IO.Path.Combine(BH.Engine.Base.Query.BHoMFolder(), fileName + ".dll");
                if (System.IO.File.Exists(assemblyPath))
                    assemblies.Add(BH.Engine.Base.Compute.LoadAssembly(assemblyPath));
            }
            return assemblies;
        }
    }
}
