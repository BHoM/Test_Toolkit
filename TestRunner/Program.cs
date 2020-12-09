using BH.oM.Reflection.Debugging;
using BH.oM.Test.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
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
                        TestResult result = method.Invoke(null, new object[] { }) as TestResult;

                        Console.WriteLine(result.Description + " --> " + result.Status.ToString());
                        foreach (Event e in result.Events.OrderBy(x => x.Message))
                            Console.WriteLine("  - " + e.Message);
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
            foreach (string file in Directory.GetFiles(@"C:\ProgramData\BHoM\Assemblies"))
            {
                if (file.EndsWith("_Test.dll"))
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFrom(file);
                        foreach (Type type in asm.GetTypes().Where(x => x.Name == "Verify" && x.Namespace.StartsWith("BH.Test")))
                        {
                            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.ReturnType == typeof(TestResult) && x.GetParameters().Count() == 0))
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

        /*************************************/
    }
}
