/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Test.Results;
using BH.oM.Test.Interoperability.Results;
using BH.oM.UnitTest.Results;
using BH.Engine.Test;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static TestResult ComputePushPullCompareDifferences(List<TestResult> setResults, List<TestResult> refResults)
        {
            TestResult fullResult = new TestResult { Description = "PushPullCompare difference" };

            List<TestResult> refResultsCopy = new List<TestResult>(refResults);

            foreach (TestResult result in setResults)
            {
                TestResult refRes = refResultsCopy.Where(x => x.ID == result.ID).FirstOrDefault();

                if (refRes == null)
                {
                    fullResult.Information.Add(refRes.NoReferenceFound());
                }
                else
                {
                    refResultsCopy.Remove(refRes);
                    fullResult.Information.Add(Compare(result, refRes));
                }
            }

            foreach (TestResult refObjRes in refResultsCopy)
            {
                fullResult.Information.Add(refObjRes.IOnlyReferenceFound());
            }

            fullResult.Status = fullResult.Information.MostSevereStatus();

            if (fullResult.Status == oM.Test.TestStatus.Error)
                fullResult.Message = "Differences have been introduced, made worse or have changed in at least one of the result sets.";
            else if (fullResult.Status == oM.Test.TestStatus.Pass)
                fullResult.Message = "No new differences have been introduced.";
            else
                fullResult.Message = "Some differences have either been removed or probably improved. The change is probably for the better but needs to be validated.";

            var properties = fullResult.ExceptionProperties(true, true);

            fullResult.Message += Environment.NewLine + "Error properties: ";

            foreach (string errorProp in properties.Item1)
            {
                fullResult.Message += errorProp + ", ";
            }

            fullResult.Message = fullResult.Message.TrimEnd(' ', ',');

            fullResult.Message += Environment.NewLine + "Warning properties: ";

            foreach (string errorProp in properties.Item2)
            {
                fullResult.Message += errorProp + ", ";
            }

            fullResult.Message = fullResult.Message.TrimEnd(' ', ',');

            return fullResult;

        }

        /***************************************************/

    }
}
