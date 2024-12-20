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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.Engine.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using UT = BH.oM.Test.UnitTests;
using BH.oM.Test.Results;
using BH.Engine.Base;
using BH.oM.UnitTest.Results;
using BH.Engine.Test;
using BH.oM.Data.Library;

using System.IO;
using BH.Engine.Diffing;

namespace BH.Engine.UnitTest
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Executes all unit tests stored in a serialised file of test sets.")]
        [Input("fileName", "The full file path to the file containing the serialised test datasets.")]
        [Output("result", "Results from the comparison of the run data with the expected output.")]
        public static TestResult CheckTest(this string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
                return new TestResult() { Status = BH.oM.Test.TestStatus.Warning, Message = "Provided file name was null." };

            StreamReader sr = new StreamReader(fileName);
            string line = sr.ReadToEnd();
            sr.Close();

            object ds = BH.Engine.Serialiser.Convert.FromJson(line);
            if(ds == null)
                return new TestResult() { Status = BH.oM.Test.TestStatus.Warning, Message = "Dataset did not deserialise correctly." };

            Dataset testSet = ds as Dataset;
            if(testSet == null)
                return new TestResult() { Status = BH.oM.Test.TestStatus.Warning, Message = "Dataset did not deserialise correctly as a BHoM Dataset." };

            return CheckTests(testSet);
        }

        [Description("Executes all unit tests in a dataset and returns a total TestResult from the execution of all testResults in the dataset.")]
        [Input("testDataSet", "The test dataset to be evaluated.")]
        [Output("results", "Results from the comparison of the run data with the expected output.")]
        public static TestResult CheckTests(this Dataset testDataSet)
        {
            if (testDataSet == null)
                return new TestResult { Status = oM.Test.TestStatus.Error, Description = "Unit test set", Message = "The provided Dataset was null and could not be evaluated." };

            string name = !string.IsNullOrWhiteSpace(testDataSet.Name) ? testDataSet.Name : !string.IsNullOrWhiteSpace(testDataSet.SourceInformation?.Title) ? testDataSet.SourceInformation.Title : "Unnamed dataset";

            string description = $"Unit test set: {name}.";

            if (!string.IsNullOrWhiteSpace(testDataSet.SourceInformation?.Author))
                description += $" Author: {testDataSet.SourceInformation.Author}";

            TestResult testResult = new TestResult { Description = description };

            List<UT.UnitTest> unitTests = testDataSet.Data.OfType<UT.UnitTest>().ToList();

            if (unitTests.Count == 0)
            {
                testResult.Status = oM.Test.TestStatus.Error;
                testResult.Message = "The provided Dataset does not contain any UnitTests.";
                return testResult;
            }

            foreach (UT.UnitTest test in unitTests)
            {
                testResult.Information.Add(CheckTest(test));
            }

            testResult.Status = testResult.Information.MostSevereStatus();

            if (unitTests.Count != testDataSet.Data.Count)
            {
                testResult.Message = "The Dataset contains other data than UnitTests." + Environment.NewLine;

                //Update the status to warning unless it is already set to the more severe level
                if (testResult.Status != oM.Test.TestStatus.Error)
                    testResult.Status = oM.Test.TestStatus.Warning;
            }

            if (testResult.Status == oM.Test.TestStatus.Error)
                testResult.Message += "One or more of the UnitTests failed.";
            else if (testResult.Status == oM.Test.TestStatus.Pass)
                testResult.Message += "No errors or warnings reported from running any of the UnitTest.";
            else
                testResult.Message += "Warnings reported during the execution of the unit test.";

            return testResult;
        }

        /***************************************************/

        [Description("Executes a unit test and compares the result of running the method with provided test data with the corresponding expected outputs.")]
        [Input("test", "The test to run, containing the method to be executed as well as the data to test on.")]
        [Output("result", "Results from the comparison of the run data with the expected output.")]
        public static TestResult CheckTest(this UT.UnitTest test)
        {
            if (test == null)
                return new TestResult { Status = oM.Test.TestStatus.Error, Description = "UnitTest", Message = "The provided UnitTest was null and could not be evaluated." };

            MethodBase method = test.Method;

            if (method == null)
                return new TestResult { Status = oM.Test.TestStatus.Error, Description = "UnitTest", Message = "The method of the provided UnitTest was null and could not be evaluated." };


            string description = $"UnitTest: Method: {method.ToText()}" + (!string.IsNullOrWhiteSpace(test.Name) ? $" ,name: {test.Name}." : ".");

            TestResult testResult = new TestResult { Description = description };

            int counter = 0;
            foreach (UT.TestData data in test.Data)
            {
                testResult.Information.Add(CheckTest(method, data, counter));
                counter++;
            }

            testResult.Status = testResult.Information.MostSevereStatus();

            if (testResult.Status == oM.Test.TestStatus.Error)
                testResult.Message = "The UnitTest did not pass.";
            else if (testResult.Status == oM.Test.TestStatus.Pass)
                testResult.Message = "No errors or warnings were reported from running the UnitTest.";
            else
                testResult.Message = "Warnings were reported during the execution of the UnitTest.";

            return testResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static TestResult CheckTest(MethodBase method, UT.TestData data, int index)
        {
            if (data == null)
                return new TestResult { Status = oM.Test.TestStatus.Error, Description = "TestData", Message = "The provided TestData was null and could not be evaluated." };

            string description = "TestData: " + (!string.IsNullOrWhiteSpace(data.Name) ? $"name: {data.Name}," : "") + $"index: {index}";
            TestResult testResult = new TestResult { Description = description };
            var result = Run(method, data);

            //Check if critical errors where raised while running the unit test
            if (result.Item2.Count != 0)
            {
                testResult.Status = oM.Test.TestStatus.Error;
                testResult.Message = "Failed to run unit test. Errors given: ";
                foreach (string error in result.Item2)
                {
                    testResult.Message += Environment.NewLine + error;
                }
                return testResult;
            }

            try
            {
                if (result.Item1.Count != data.Outputs.Count)
                {
                    testResult.Status = oM.Test.TestStatus.Error;
                    testResult.Message = "Execution of the method returned a different number of results compared to the expected output.";
                    return testResult;
                }

                List<ComparisonDifference> differences = CompareResults(data.Outputs, result.Item1);

                if (differences.Count == 0)
                {
                    testResult.Status = oM.Test.TestStatus.Pass;
                    testResult.Message = "Execution of the method returned the expected outputs.";
                }
                else
                {
                    testResult.Status = oM.Test.TestStatus.Error;
                    testResult.Message = "Execution of the method did not return the expected outputs.";
                    testResult.Information = differences.Cast<BH.oM.Test.ITestInformation>().ToList();
                }

                return testResult;
            }
            catch(Exception e)
            {
                testResult.Status = oM.Test.TestStatus.Error;
                testResult.Message = "Failed to run the comparison of the result of execution of the method with the expected outputs." + Environment.NewLine;
                testResult.Message += $"Exception thrown: {e.Message}";
                return testResult;
            }
            
        }

        /***************************************************/

        private static List<ComparisonDifference> CompareResults(IList<object> referenceData, IList<object> runData)
        {
            ComparisonConfig comparisonConfig = Engine.Test.Create.DefaultTestComparisonConfig();

            List<ComparisonDifference> differences = new List<ComparisonDifference>();

            for (int i = 0; i < referenceData.Count; i++)
            {
                object runObj = runData[i];
                object refObject = referenceData[i];

                if (runObj == null)
                {
                    if (refObject == null)
                        continue;
                    else
                    {
                        differences.Add(new ComparisonDifference()
                        {
                            Property = "",
                            ReferenceValue = refObject,
                            RunValue = null,
                            Status = oM.Test.TestStatus.Error,
                            Message = $"Data returned for output {i} is an unexpected null."
                        });
                    }

                }
                runObj = CastToObjectType(runObj, refObject);
                

                var diffResult = refObject.ObjectDifferences(runObj, comparisonConfig);

                for (int j = 0; j < diffResult.Differences.Count; j++)
                {
                    differences.Add(new ComparisonDifference()
                    {
                        Property = diffResult.Differences[j].FullName,
                        ReferenceValue = diffResult.Differences[j].PastValue,
                        RunValue = diffResult.Differences[j].FollowingValue,
                        Status = oM.Test.TestStatus.Error,
                        Message = $"Data returned for output {i} is different than expected."
                    });
                }
            }

            return differences;
        }

        /***************************************************/

        private static object CastToObjectType(object item, object target)
        {
            if (item == null || target == null)
                return item;

            try
            {
                Type type = target.GetType();
                if (type.IsGenericType && item is IEnumerable && target is IEnumerable)
                {
                    dynamic list;
                    if (type.IsInterface)
                        list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GenericTypeArguments[0]));
                    else
                        list = Activator.CreateInstance(type);

                    IEnumerator itemEnum = (item as IEnumerable).GetEnumerator();
                    IEnumerator targetEnum = (target as IEnumerable).GetEnumerator();

                    while (itemEnum.MoveNext())
                    {
                        dynamic val = itemEnum.Current;

                        if (targetEnum.MoveNext() && val is IEnumerable)
                            list.Add(CastToObjectType(val, targetEnum.Current));
                        else
                            list.Add(val);
                    }
                    return list;
                }
                else
                {
                    return item;
                }
            }
            catch
            {
                return item;
            }
        }

    }
}




