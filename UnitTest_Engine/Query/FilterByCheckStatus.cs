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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.Engine.Serialiser;
using BH.oM.Base;
using UT = BH.oM.Test.UnitTests;
using BH.oM.Test.Results;
using BH.Engine.Base;
using BH.oM.UnitTest.Results;
using BH.Engine.Test;
using BH.oM.Data.Library;
using System.IO;

namespace BH.Engine.UnitTest
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Runs and checks all provided UnitTests and provides the ones that has a the provided status.")]
        [Input("test", "The tests to filter.")]
        [Input("status", "Status looked for.")]
        [Input("filterData", "If true, returns new UnitTests objects where the data has been filtered to only contain TestData which returns the provided status. If false, returns full provided UnitTest if the overall status is matching the provided status.")]
        [MultiOutput(0, "matchingTests", "The filtered tests with status matching the provided status.")]
        [MultiOutput(1, "matchingResults", "Results matching the filtered tests.")]
        [MultiOutput(2, "unmatchingTests", "Tests with a status other than the provided.")]
        [MultiOutput(3, "unmatchingResults", "TestResults corresponding to the UnitTests not matching the status.")]
        public static Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> FilterByCheckStatus(this List<UT.UnitTest> tests, BH.oM.Test.TestStatus status = oM.Test.TestStatus.Error, bool filterData = false)
        {
            tests = tests.Where(x => x != null).ToList();   //Filter out nulls

            //Return collections
            List<UT.UnitTest> matchingTests = new List<UT.UnitTest>();
            List<TestResult> matchingResults = new List<TestResult>();

            List<UT.UnitTest> unmatchingTests = new List<UT.UnitTest>();
            List<TestResult> unmatchingResults = new List<TestResult>();

            if (filterData) //If filterData is true, some work is required
            {
                foreach (UT.UnitTest test in tests) //Loop through all tests
                {
                    TestResult result = test.CheckTest();   //Get result from check test

                    if (result.Information.All(x => x.Status == status))    //If all inner reuslts match the status, the full test and result can be outputed. Nothing to add to the unmatching
                    {
                        matchingTests.Add(test);
                        matchingResults.Add(result);
                    }
                    else    //If not, need to investigate inner
                    {
                        //Set up output results for matches
                        UT.UnitTest matchTest = new UT.UnitTest { Method = test.Method };  //Halfclone of the UT, keeping only the method
                        TestResult matchResult = result.ShallowClone();    //Clone the result and wipe inner TestInformation
                        matchResult.Information = new List<oM.Test.ITestInformation>();

                        //Set up outputs for non-matches
                        UT.UnitTest unmatchTest = new UT.UnitTest { Method = test.Method };  //Halfclone of the UT, keeping only the method
                        TestResult unmatchResult = result.ShallowClone();    //Clone the result and wipe inner TestInformation
                        unmatchResult.Information = new List<oM.Test.ITestInformation>();

                        for (int i = 0; i < test.Data.Count; i++)   //For each innner TestData point
                        {
                            if (result.Information[i].Status == status) //Status matching -> add Data to UT and test information to result
                            {
                                matchTest.Data.Add(test.Data[i]);
                                matchResult.Information.Add(result.Information[i]);
                            }
                            else    //Status not matching -> Add data and TestInformation no unmatching entries
                            {
                                unmatchTest.Data.Add(test.Data[i]);
                                unmatchResult.Information.Add(result.Information[i]);
                            }
                        }

                        if (matchTest.Data.Count != 0) //At least one inner match found
                        {
                            matchingTests.Add(matchTest);
                            matchingResults.Add(matchResult);
                        }

                        if (unmatchTest.Data.Count != 0) //At least one inner unmatch found
                        {
                            unmatchingTests.Add(unmatchTest);
                            unmatchingResults.Add(unmatchResult);
                        }
                    }
                }
            }
            else    //If not filtering by data, we are only interesed in the outer test result status
            {
                for (int i = 0; i < tests.Count; i++)   //Simly loop through the tests
                {
                    TestResult result = tests[i].CheckTest();
                    if (result.Status == status)    //And add if matching outer status
                    {
                        matchingTests.Add(tests[i]);
                        matchingResults.Add(result);
                    }
                    else    //If not, add to unmatch list
                    {
                        unmatchingTests.Add(tests[i]);
                        unmatchingResults.Add(result);
                    }
                }
            }

            return new Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> { Item1 = matchingTests, Item2 = matchingResults, Item3 = unmatchingTests, Item4 = unmatchingResults };
        }

        /***************************************************/

        [Description("Runs and checks all provided UnitTests in the provided Dataset and provides the ones that has a the provided status.")]
        [Input("testDataSet", "The test dataset to be evaluated.")]
        [Input("status", "Status looked for.")]
        [Input("filterData", "If true, returns new UnitTests objects where the data has been filtered to only contain TestData which returns the provided status. If false, returns full provided UnitTest if the overall status is matching the provided status.")]
        [MultiOutput(0, "matchingTests", "The filtered tests with status matching the provided status.")]
        [MultiOutput(1, "matchingResults", "Results matching the filtered tests.")]
        [MultiOutput(2, "unmatchingTests", "Tests with a status other than the provided.")]
        [MultiOutput(3, "unmatchingResults", "TestResults corresponding to the UnitTests not matching the status.")]
        public static Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> FilterByCheckStatus(this Dataset testDataSet, BH.oM.Test.TestStatus status = oM.Test.TestStatus.Error, bool filterData = false)
        {
            if (testDataSet == null)
                return new Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> { Item1 = new List<UT.UnitTest>(), Item2 = new List<TestResult>(), Item3 = new List<UT.UnitTest>(), Item4 = new List<TestResult>() };
                
            List<UT.UnitTest> unitTests = testDataSet.Data.OfType<UT.UnitTest>().ToList();
            return FilterByCheckStatus(unitTests, status, filterData);
        }

        /***************************************************/

        [Description("Runs and checks all provided UnitTests and provides the ones that has a the provided status.")]
        [Input("fileName", "The full file path to the file containing the serialised test datasets.")]
        [Input("status", "Status looked for.")]
        [Input("filterData", "If true, returns new UnitTests objects where the data has been filtered to only contain TestData which returns the provided status. If false, returns full provided UnitTest if the overall status is matching the provided status.")]
        [MultiOutput(0, "matchingTests", "The filtered tests with status matching the provided status.")]
        [MultiOutput(1, "matchingResults", "Results matching the filtered tests.")]
        [MultiOutput(2, "unmatchingTests", "Tests with a status other than the provided.")]
        [MultiOutput(3, "unmatchingResults", "TestResults corresponding to the UnitTests not matching the status.")]
        public static Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> FilterByCheckStatus(this string fileName, BH.oM.Test.TestStatus status = oM.Test.TestStatus.Error, bool filterData = false)
        {
            Dataset testSet = DatasetFromFile(fileName);
            if (testSet == null)
                return new Output<List<UT.UnitTest>, List<TestResult>, List<UT.UnitTest>, List<TestResult>> { Item1 = new List<UT.UnitTest>(), Item2 = new List<TestResult>(), Item3 = new List<UT.UnitTest>(), Item4 = new List<TestResult>() };
            
            return FilterByCheckStatus(testSet, status, filterData);
        }

        /***************************************************/
    }
}




