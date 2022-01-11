/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.oM.Base;
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

        [Description("Gets a TestResult result back that correspond to an information only found in the reference set when compared to data just run.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.\n" + 
                     "Recursivly adds any inner TestInformation as data only found in reference.")]
        [Input("refResult", "The reference TestResult only found in the reference data.")]
        [Output("result", "A TestResult noting the fact that the data was only found in the reference set.")]
        public static TestResult OnlyReferenceFound(this TestResult refResult)
        {
            if (refResult == null)
                return null;

            return new TestResult
            {
                Description = $"PushPullCompare difference: {refResult.ID}",
                Message = $"Result only found in reference data with ID {refResult.ID}. No comparison available from run.",
                ID = refResult.ID,
                Status = oM.Test.TestStatus.Warning,
                Information = refResult.NonEventMessageInformation().Select(x => IOnlyReferenceFound(x)).Where(x => x != null).ToList()
            };
        }

        /***************************************************/

        [Description("Gets a ComparisonDifference result back that correspond to an information only found in the reference set when compared to data just run.\n" +
                     "For PushPullObjectComparison this can mean that a bug in the adapter converters has been fixed and that a difference is no longer present between pushed and pulled object.")]
        [Input("refResult", "The reference PushPullObjectComparison only found in the reference data.")]
        [Output("result", "A TestResult noting the fact that the data was only found in the reference set.")]
        public static ComparisonDifference OnlyReferenceFound(this PushPullObjectComparison refResult)
        {
            if (refResult == null)
                return null;

            return new ComparisonDifference
            {
                Message = "Difference only found in reference.",
                ReferenceValue = refResult.ReturnedItem,
                Property = refResult.PropertyID,
                Status = oM.Test.TestStatus.Warning,
            };
        }

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Gets a TestInformation result back that correspond to an information only found in the reference set when compared to data just run.\n" +
                     "Used when comparing to TestResults against each other and information in the reference can not be found in the new data.")]
        [Input("refResult", "The reference ITestInformation only found in the reference data.")]
        [Output("result", "A test information noting the fact that the data was only found in the reference set.")]
        public static ITestInformation IOnlyReferenceFound(this ITestInformation refResult)
        {
            if (refResult == null)
                return null;

            return OnlyReferenceFound(refResult as dynamic);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static ITestInformation OnlyReferenceFound(ITestInformation result)
        {
            return null;
        }

        /***************************************************/
    }
}

