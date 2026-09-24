/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Base.Debugging;
using BH.oM.Test.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Test;
using BH.Engine.Test;

namespace TestRunner
{
    class Program
    {
        /*************************************/
        /**** Module Initialiser          ****/
        /*************************************/

        // Registered as a module initialiser so it runs before Main is JIT-compiled, i.e. before the runtime
        // needs to resolve BHoM/Test_oM types referenced by Main's own locals. This lets TestRunner run
        // without bundling its own copies of the BHoM DLLs, always resolving them from the Assemblies folder
        // so it tests the actual DLLs used by the BHoM UI rather than a stale local copy.
        [ModuleInitializer]
        internal static void RegisterAssemblyResolution()
        {
            AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
            {
                string path = Path.Combine(m_AssembliesFolder, assemblyName.Name + ".dll");
                return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
            };
        }


        /*************************************/
        /**** Main                        ****/
        /*************************************/

        static void Main(string[] args)
        {
            LoadAllTestAssemblies();

            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a filter for the methods you want to run. This can be either the name of the method or its namespace (after 'BH.Test')");
                return;
            }

            string key = args[0];
            if (!m_TestMethods.ContainsKey(key))
            {
                Console.WriteLine("Cannot find any test matching " + key);
            }
            else
            {
                foreach (MethodInfo method in m_TestMethods[key])
                {
                    try
                    {
                        object[] parameters = new object[] { };
                        if (method.GetParameters().Length == 1)
                        {
                            if (args.Length == 2)
                            {
                                parameters = new object[] { System.Convert.ToBoolean(args[1]) };
                            }
                            else if (method.GetParameters()[0].HasDefaultValue)
                            {
                                parameters = new object[] { method.GetParameters()[0].DefaultValue };
                            }
                        }
                        TestResult result = method.Invoke(null, parameters) as TestResult;
                        Console.WriteLine();
                        Console.Write(result.FullMessage());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Method {method.Name} failed to run:\n{e.Message}");
                    }
                }
            }
        }


        /*************************************/
        /**** Private Method              ****/
        /*************************************/

        static void LoadAllTestAssemblies()
        {
            foreach (string file in Directory.GetFiles(m_AssembliesFolder))
            {
                if (file.EndsWith("_Test.dll"))
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFrom(file);
                        foreach (Type type in asm.GetTypes().Where(x => x.Name == "Verify" && x.Namespace.StartsWith("BH.Test")))
                        {
                            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.ReturnType == typeof(TestResult) && (x.GetParameters().Count() == 0 || (x.GetParameters().Count() == 1 && x.GetParameters()[0].ParameterType == typeof(bool)))))
                                RegisterTestMethod(method);
                        }
                    }
                    catch {}
                }
            }
        }

        /*************************************/

        static void RegisterTestMethod(MethodInfo method)
        {
            List<string> keys = new List<string> { method.Name };

            string[] path = method.DeclaringType.Namespace.Split(new char[] { '.' });
            if (path.Length >= 3)
                keys.Add(path[2]);

            foreach (string key in keys)
            {
                if (!m_TestMethods.ContainsKey(key))
                    m_TestMethods[key] = new List<MethodInfo> { method };
                else
                    m_TestMethods[key].Add(method);
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        static Dictionary<string, List<MethodInfo>> m_TestMethods = new Dictionary<string, List<MethodInfo>>();

        static readonly string m_AssembliesFolder = @"C:\ProgramData\BHoM\Assemblies";

        /*************************************/
    }
}





