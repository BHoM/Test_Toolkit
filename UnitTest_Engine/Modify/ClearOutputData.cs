/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using UT = BH.oM.Test.UnitTests;

namespace BH.Engine.UnitTest
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Clears all the stored outputs from each TestData item in the data of the provided UnitTest.")]
        [Input("test", "The test to clear all output data from.")]
        [Output("test", "Test with original method and inputs, but unassigned outputs.")]
        public static UT.UnitTest ClearOutputData(this UT.UnitTest test)
        {
            if(test == null)
            {
                BH.Engine.Base.Compute.RecordError("Test cannot be null when clearing output data.");
                return test;
            }

            List<UT.TestData> data = new List<UT.TestData>();

            foreach (UT.TestData inData in test.Data)
            {
                data.Add(new UT.TestData(inData.Inputs, new List<object>()));
            }

            return new UT.UnitTest
            {
                Method = test.Method,
                Data = data,
                Name = test.Name,
                CustomData = new Dictionary<string, object>(test.CustomData),
                Tags = new HashSet<string>(test.Tags),
                Fragments = new FragmentSet(test.Fragments)
            };
        }

        /***************************************************/
    }
}




