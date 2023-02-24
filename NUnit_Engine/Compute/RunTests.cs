/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using BH.oM.Test.NUnit;
using NUnit.Engine;
using System.Xml.Serialization;
using BH.oM.Base.Attributes;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace BH.Engine.Test.NUnit
{
    public static partial class Compute
    {
        [Description("Runs a set of NUnit tests from a given DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            // Load assemblies referenced by this assembly.
            // Required to ensure that all dependencies (including dynamically-loaded assemblies) are present for the test run, regardless of the executing environment.
            // In a normal test run in VS (not via NUnit.Engine) this is taken care of from the base BH.oM.Test.NUnit.NUnitTest class.
            LoadReferencedAssemblies(filePath);

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);
            var testRunner = testEngine.GetRunner(package);
            var testResult = testRunner.Run(null, TestFilter.Empty);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(testResult.OuterXml);

            XmlSerializer ser = new XmlSerializer(typeof(TestRun));
            TestRun result = null;
            using (XmlNodeReader reader = new XmlNodeReader(xmlDoc))
            {
                result = ser.Deserialize(reader) as TestRun;
            }
            return result;
        }

        [Description("Makes sure that the assemblies referenced by the input assembly are loaded in memory." +
            "Required to ensure that all dependencies (including dynamically-loaded assemblies) are present for the test run, regardless of the executing environment.")]
        private static void LoadReferencedAssemblies(string dllPath)
        {
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            assembliesToCheck.Enqueue(Assembly.LoadFile(dllPath));

            while (assembliesToCheck.Any())
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();

                var referencedAssemblies = assemblyToCheck.GetReferencedAssemblies();

                foreach (var reference in referencedAssemblies)
                {
                    if (!loadedAssemblies.Contains(reference.FullName))
                    {
                        var assembly = Assembly.Load(reference);
                        assembliesToCheck.Enqueue(assembly);
                        loadedAssemblies.Add(reference.FullName);
                    }
                }
            }
        }
    }
}
