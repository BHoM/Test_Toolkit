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

        [Description("Gets a TestResult back that correspond to no reference data was available that matching the result.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.\n" +
                     "Recursivly adds any inner TestInformation as no reference found.")]
        [Input("result", "The TestResult for which no reference data could be found.")]
        [Output("result", "A TestResult noting the fact that the data was only found in the reference set.")]
        public static TestResult NoReferenceFound(this TestResult result)
        {
            if (result == null)
                return null;

            return new TestResult
            {
                Description = "PushPullCompare difference",
                Message = $"No reference results could be found for {result.ID}",
                ID = result.ID,
                Status = oM.Test.TestStatus.Warning,
                Information = result.NonEventMessageInformation().Select(x => INoReferenceFound(x)).Where(x => x != null).ToList()
            };
        }

        /***************************************************/

        [Description("Gets a ComparisonDifference back that correspond to no reference data was available that matches the result, which generally means that a difference has been introduced.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.")]
        [Input("result", "The PushPullObjectComparison for which no reference data could be found.")]
        [Output("result", "A ComparisonDifference noting the fact that the data was only found in the reference set and a difference probably has been introduced in the adapter code.")]
        public static ComparisonDifference NoReferenceFound(this PushPullObjectComparison result)
        {
            if (result == null)
                return null;

            //No avilable reference
            return new ComparisonDifference
            {
                Message = "A difference has been introduced.",
                Status = oM.Test.TestStatus.Error,
                Property = result.PropertyID,
                RunValue = result.ReturnedItem
            };
        }


        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Gets a ITestInformation back that correspond to no reference data was available that matching the result.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.")]
        [Input("result", "The ITestInformation for which no reference data could be found.")]
        [Output("result", "A ITestInformation noting the fact that the data was only found in the reference set.")]
        public static ITestInformation INoReferenceFound(this ITestInformation result)
        {
            if (result == null)
                return null;

            return NoReferenceFound(result as dynamic);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static ITestInformation NoReferenceFound(ITestInformation result)
        {
            return new TestResult
            {
                Description = "PushPullCompare difference",
                Message = $"No reference results could be found for test result information of type " + result.GetType(),
                Status = oM.Test.TestStatus.Error
            };
        }

        /***************************************************/
    }
}
