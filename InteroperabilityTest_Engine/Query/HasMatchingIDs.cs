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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using BH.oM.Base;
using BH.oM.Test.Results;
using BH.oM.Test.Interoperability.Results;
using BH.oM.Test;

namespace BH.Engine.Test.Interoperability
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks if the two TestResult has the same ID. Used for comparing two test results with each other.")]
        [Input("a", "First TestResult to compare.")]
        [Input("b", "Second TestResult to compare.")]
        [Output("sameID", "Returns true if the two TestResult have the same ID.")]
        public static bool HasMatchingIDs(this TestResult a, TestResult b)
        {
            if (a == null || b == null)
                return false;

            return a.ID == b.ID;
        }

        /***************************************************/

        [Description("Checks if the two PushPullObjectComparison has the same PropertyID. Used for comparing two test results with each other.")]
        [Input("a", "First PushPullObjectComparison to compare.")]
        [Input("b", "Second PushPullObjectComparison to compare.")]
        [Output("sameID", "Returns true if the two test informations have the same PropertyID.")]
        public static bool HasMatchingIDs(this PushPullObjectComparison a, PushPullObjectComparison b)
        {
            if (a == null || b == null)
                return false;

            return a.PropertyID == b.PropertyID;
        }

        /***************************************************/
        /**** Public Methods - Interface                ****/
        /***************************************************/

        [Description("Checks if the two test infonformations has the same ID. Type of ID used will depend on type. Used for comparing two test results with each other.")]
        [Input("a", "First test information to compare.")]
        [Input("b", "Second test information to compare.")]
        [Output("sameID", "Returns true if the two test informations have the same ID.")]
        public static bool IHasMatchingIDs(this ITestInformation a, ITestInformation b)
        {
            if (a == null || b == null)
                return false;

            return HasMatchingIDs(a as dynamic, b as dynamic);
        }

        /***************************************************/
        /**** Private Methods - Fallback                ****/
        /***************************************************/

        private static bool HasMatchingIDs(this ITestInformation a, ITestInformation b)
        {
            //Different types or class not supported for the method -> false
            return false;
        }

        /***************************************************/
    }
}


