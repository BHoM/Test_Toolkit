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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Test.Results;
using BH.oM.Test.Interoperability.Results;
using BH.oM.UnitTest.Results;
using BH.oM.Test;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets a ComparisonDifference back that correspond to a situation where the reference data was showing a crash no longer present.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.\n" +
                     "This is generally a good situation, but the data now able to be pushed needs to be validated.\n" +
                     "Recursivly adds any inner TestInformation as NewResultAfterCrashFix.")]
        [Input("result", "The PushPullObjectComparison no longer showing a crash.")]
        [Output("result", "A ComparisonDifference noting the fact that the reference was showing a crash while new results is not.")]
        public static TestResult NewResultAfterCrashFix(this TestResult result)
        {
            if (result == null)
                return null;

            return new TestResult
            {
                Description = "PushPullCompare difference",
                Message = $"A previous crash in the reference has been fixed and is now showing warnings for test result with ID: {result.ID}",
                ID = result.ID,
                Status = TestStatus.Warning,
                Information = result.NonEventMessageInformation().Select(x => INewResultAfterCrashFix(x)).Where(x => x != null).ToList()
            };
        }

        /***************************************************/

        [Description("Gets a ComparisonDifference back that correspond to a situation where the reference data was showing a crash no longer present.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.\n" + 
                     "This is generally a good situation, but the data now able to be pushed needs to be validated.")]
        [Input("result", "The PushPullObjectComparison no longer showing a crash.")]
        [Output("result", "A ComparisonDifference noting the fact that the reference was showing a crash while new results is not.")]
        public static ComparisonDifference NewResultAfterCrashFix(this PushPullObjectComparison result)
        {
            if (result == null)
                return null;

            return new ComparisonDifference
            {
                Message = "Difference now showing after previous crash.",
                ReferenceValue = result.ReturnedItem,
                Property = result.PropertyID,
                Status = oM.Test.TestStatus.Warning
            };
        }

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Gets a ITestInformation back that correspond to a situation where the reference data was showing a crash no longer present.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.\n")]
        [Input("result", "The ITestInformation no longer showing a crash.")]
        [Output("result", "A ITestInformation noting the fact that the reference was showing a crash while new results is not.")]
        public static ITestInformation INewResultAfterCrashFix(this ITestInformation result)
        {
            if (result == null)
                return null;

            return NewResultAfterCrashFix(result as dynamic);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static ITestInformation NewResultAfterCrashFix(ITestInformation result)
        {
            return new TestResult
            {
                Description = "PushPullCompare difference",
                Message = $"A previous crash in the reference has been fixed and is now showing warnings for test result information of type " + result.GetType(),
                Status = TestStatus.Warning
            };
        }

        /***************************************************/
    }
}
