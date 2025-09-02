//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Text;


//namespace BH.Tests.Setup.TestBases
//{
//    [SetUpFixture]
//    public class GlobalTestSetup
//    {
//        private const string BHoM_Assemblies_Path = @"C:\ProgramData\BHoM\Assemblies";
//        static GlobalTestSetup()
//        {
//            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
//            {
//                string assemblyPath = System.IO.Path.Combine(BHoM_Assemblies_Path, new System.Reflection.AssemblyName(args.Name).Name + ".dll");
//                if (System.IO.File.Exists(assemblyPath))
//                    return System.Reflection.Assembly.LoadFrom(assemblyPath);
//                return null;
//            };

//        }
//    }
//}



// GlobalInit.cs
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

#if NETCOREAPP || NET5_0_OR_GREATER
using System.Runtime.Loader;
#endif

internal static class GlobalInit
{
    private const string BHoM_Assemblies_Path = @"C:\ProgramData\BHoM\Assemblies";

    [ModuleInitializer]
    public static void Initialize()
    {
#if NETFRAMEWORK
        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var requested = new AssemblyName(args.Name).Name + ".dll";
            var path = Path.Combine(BHoM_Assemblies_Path, requested);
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
#else
        // On .NET (Core/5+), use AssemblyLoadContext instead of AppDomain.AssemblyResolve.
        AssemblyLoadContext.Default.Resolving += (_, assemblyName) =>
        {
            var path = Path.Combine(BHoM_Assemblies_Path, assemblyName.Name + ".dll");
            return File.Exists(path)
                ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path)
                : null;
        };
#endif
    }
}
