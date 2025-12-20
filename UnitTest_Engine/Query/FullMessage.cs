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
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;
using BH.oM.UnitTest.Results;
using BH.oM.Test;
using BH.Engine.Test;

namespace BH.Engine.UnitTest
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns a full concatenated message for the ComparisonDifference, only giving messages worse or equal to the provided severity level.")]
        [Input("result", "The ComparisonDifference to give the full message for.")]
        [Input("maxDepth", "Maximum level of recursiveness for inner TestInformation. Not in use for this object type.")]
        [Input("minSeverity", "The minimum level of severity of the information to report back. Returns an empty string if the ComparisonDifference does not pass this check.")]
        [Output("message", "Full message for the ComparisonDifference.")]
        public static string FullMessage(ComparisonDifference difference, int maxDepth = 3, TestStatus minSeverity = TestStatus.Pass)
        {
            if (difference == null || !difference.Status.IsEqualOrMoreSevere(minSeverity))
                return "";

            string message = difference.Message + Environment.NewLine;
            message += $"Expected value: {difference.ReferenceValue}, returned value from execution: {difference.RunValue}";

            if (string.IsNullOrEmpty(difference.Property))
                message += ".";
            else
                message += $", for the property {difference.Property} of the returned object.";

            message += Environment.NewLine;

            return message;
        }

        /***************************************************/
    }
}





