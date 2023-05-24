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

using BH.oM.Base.Attributes;
using BH.oM.Test.NUnit;
using NUnit.Engine;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace BH.Engine.Test.NUnit
{
    public static partial class Compute
    {
        [Description("Runs all NUnit tests with a given method name from a given DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Input("methodName", "Full name of the test method (in format {namespace}.{class}.{method}).")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath, string methodName)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || string.IsNullOrWhiteSpace(methodName))
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);            
            var testRunner = testEngine.GetRunner(package);

            string[] split = methodName.Split('.');
            string namespaceAndClass = string.Join(".", split.Take(split.Length - 1));
            string method = split.Last();

            TestFilter tf = new TestFilter($"<filter><and><class>{namespaceAndClass}</class><method>{method}</method></and></filter>");
            var testResult = testRunner.Run(null, tf);
            return testResult.ToTestRun();
        }

        [Description("Runs all NUnit tests with a given class and method names from a given DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Input("className", "Full name of the class that contains test methods (in format {namespace}.{class}).")]
        [Input("methodNames", "Names of the test methods (in format {method}).")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath, string className, IEnumerable<string> methodNames)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || string.IsNullOrWhiteSpace(className) || (methodNames != null && !methodNames.Any()))
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);
            var testRunner = testEngine.GetRunner(package);

            TestFilter tf = new TestFilter($"<filter><and><class>{className}</class><or>{string.Join("", methodNames.Select(x => $"<method>{x}</method>"))}</or></and></filter>");
            var testResult = testRunner.Run(null, tf);
            return testResult.ToTestRun();
        }

        [Description("Runs a set of NUnit tests from a given set of classes from a DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Input("classes", "A collection of classes to run the tests from (in format {namespace}.{class}).")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath, IEnumerable<string> classes)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || classes == null || !classes.Any())
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);
            var testRunner = testEngine.GetRunner(package);

            TestFilter tf = new TestFilter($"<filter><or>{string.Join("", classes.Select(x => $"<class>{x}</class>"))}</or></filter>");
            var testResult = testRunner.Run(null, tf);
            return testResult.ToTestRun();
        }

        [Description("Runs a set of NUnit tests from a given DLL which contains code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePath);
            var testRunner = testEngine.GetRunner(package);
            var testResult = testRunner.Run(null, TestFilter.Empty);
            return testResult.ToTestRun();
        }

        [Description("Runs a set of NUnit tests from given DLLs which contain code set up with the NUnit framework.")]
        [Input("filePath", "A full file path to the DLL file which contains the NUnit tests.")]
        [Output("testRun", "The NUnit test result from running the DLL.")]
        public static TestRun RunTests(IEnumerable<string> filePaths)
        {
            if (filePaths == null)
                return null;

            filePaths = filePaths.Where(x => !string.IsNullOrWhiteSpace(x) && File.Exists(x));
            if (!filePaths.Any())
                return null;

            ITestEngine testEngine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(filePaths.ToList());
            var testRunner = testEngine.GetRunner(package);
            var testResult = testRunner.Run(null, TestFilter.Empty);
            return testResult.ToTestRun();
        }
    }
}
