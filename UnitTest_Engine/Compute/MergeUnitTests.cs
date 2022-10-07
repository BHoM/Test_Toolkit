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
using BH.oM.Test.UnitTests;
using BH.Engine.Serialiser;

namespace BH.Engine.UnitTest
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Merges all tests that share the same method into one unit test object. This is, this method will return one UnitTest per unique provided method, with the test data corresponding to all provided UnitTests of that type of method.")]
        [Input("testsToMerge", "The UnitTests to merge into unique UnitTests based on the method.")]
        [Output("mergedTests", "The list of merged UnitTests.")]
        public static List<oM.Test.UnitTests.UnitTest> MergeUnitTests(List<oM.Test.UnitTests.UnitTest> testsToMerge)
        {
            List<oM.Test.UnitTests.UnitTest> mergedTests = new List<oM.Test.UnitTests.UnitTest>();

            foreach (var group in testsToMerge.GroupBy(x => x.Method.ToJson()))
            {
                mergedTests.Add(new oM.Test.UnitTests.UnitTest { Method = group.First().Method, Data = group.SelectMany(x => x.Data).ToList() });
            }

            return mergedTests;
        }

        /***************************************************/
    }
}

