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

using BH.oM.Test.NUnit;
using NUnit.Framework;
using BH.Engine.Test.NUnit;
//using BH.oM.Test.Results;
using Newtonsoft.Json;
using Shouldly;

namespace NUnit_TestRunner_Tests
{
    public class TestsRunner_Tests
    {
        [Test]
        [Description("Uses the test runner in BH.Engine.Test.NUnit.Compute.RunTests " +
            "to run the tests written in the NUnit_Engine_Tests project and verifies that there are no errors.")]
        public void Run_NUnit_Engine_Tests()
        {
            string? solutionPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            if (solutionPath == null)
                Assert.Fail("Could not find solution directory path.");

            string NUnit_Engine_Tests_path = Path.Combine(solutionPath, "NUnit_Engine_Tests\\bin\\Debug\\net6.0\\NUnit_Engine_Tests.dll");
            
            TestRun testRunResult = Compute.RunTests(NUnit_Engine_Tests_path);
            Console.WriteLine(JsonConvert.SerializeObject(testRunResult, Formatting.Indented));

            //TestResult testResult = testRunResult.ToTestResult();
            //testResult.ShouldNotBeNull();
            //Console.WriteLine(JsonConvert.SerializeObject(testResult, Formatting.Indented));

            testRunResult.Failed.ShouldBe(0);
        }
    }
}