using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BH.Tests.Setup
{
    public static partial class Query
    {


        public static List<Assembly> InputParametersAssemblies()
        {
            if (!TestContext.Parameters.Exists("UpdatedAssemblies"))
                return null;

            if (m_AssembliesToTest != null)
                return m_AssembliesToTest;
            lock (m_AssemblyLock)
            {
                if (m_AssembliesToTest != null)
                    return m_AssembliesToTest;

                m_AssembliesToTest = new List<Assembly>();

                var assembliesUpdated = TestContext.Parameters.Get("UpdatedAssemblies", "");

                foreach (var assemblyName in assembliesUpdated.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    m_AssembliesToTest.Add(Assembly.LoadFrom(assemblyName));

                foreach (var assembly in m_AssembliesToTest)
                {
                    TestContext.WriteLine($"Assembly to test: {assembly.FullName}");
                }

                return m_AssembliesToTest;
            }
        }

        private static List<Assembly> m_AssembliesToTest = null;
        private static object m_AssemblyLock = new object();
        /***************************************************/

        public static List<string> InputParametersUpdatedFiles()
        {
            List<string> updatedFiles = new List<string>();

            if (!TestContext.Parameters.Exists("UpdatedFiles"))
                return null;

            var assembliesUpdated = TestContext.Parameters.Get("UpdatedFiles", "");

            foreach (var fileName in assembliesUpdated.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                updatedFiles.Add(Path.Combine(fileName)); //Ensure formating is correct

            return updatedFiles;
        }

        /***************************************************/


    }
}
