/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Test.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Test.NUnit
{
    public static partial class Convert
    {
        [Description("Convert an NUnit TestRun object to a BHoM TestResult object. Any TestSuite information is stored in the Information property of the TestResult.")]
        [Input("nunitTest", "The NUnit TestRun to convert.")]
        [Output("testResult", "The converted BHoM TestResult.")]
        public static TestResult ToTestResult(this TestRun nunitTest)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTest.Result);
            result.Description = $"Passed: {nunitTest.Passed}{System.Environment.NewLine}Failed: {nunitTest.Failed}{System.Environment.NewLine}";
            result.Message = $"";

            if (nunitTest.TestSuite != null)
                result = result.Merge(nunitTest.TestSuite.ToTestResult());

            return result;
        }

        [Description("Converts an NUnit TestSuite object to a BHoM TestResult object. Any child TestSuite information is stored in the Information property of the TestResult, and any TestCase information is also stored on the Information property.")]
        [Input("nunitTestSuite", "The NUnit TestSuite to convert.")]
        [Output("testResult", "The converted BHoM TestResult.")]
        public static TestResult ToTestResult(this TestSuite nunitTestSuite)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTestSuite.Result);
            result.Description = $"Passed: {nunitTestSuite.Passed}{System.Environment.NewLine}Failed: {nunitTestSuite.Failed}{System.Environment.NewLine}";
            result.Message = $"";

            if (nunitTestSuite.Children != null)
            {
                foreach (var child in nunitTestSuite.Children)
                    result = result.Merge(child.ToTestResult());
            }

            if (nunitTestSuite.TestCases.Count > 0)
                result.Information.AddRange(nunitTestSuite.TestCases.Select(x => x.ToTestResult()));

            return result;
        }

        [Description("Converts an NUnit TestResult object to a BHoM TestResult object. If the TestCase is a fail then the failure message is provided with the stack trace from NUnit.")]
        [Input("nunitTestCase", "The NUnit TestCase to convert.")]
        [Output("testResult, The converted BHoM TestResult.")]
        public static TestResult ToTestResult(this TestCase nunitTestCase)
        {
            TestResult result = new TestResult();

            result.Status = ToTestStatus(nunitTestCase.Result);
            result.Description = nunitTestCase.FullName;
            result.Message = $"";

            if(nunitTestCase.Failure != null)
                result.Message += $"{nunitTestCase.Failure.Message}{System.Environment.NewLine}{nunitTestCase.Failure.Stacktrace}";

            return result;
        }
    }
}


