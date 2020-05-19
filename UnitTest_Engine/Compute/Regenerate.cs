/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.Engine.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using UT = BH.oM.Test.UnitTests;
using BH.oM.Reflection.Interface;
using BH.oM.Reflection;

namespace BH.Engine.UnitTest
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Runs a unit test with inputs in the testdata, and construct new testdata with the result from the execution of the method. Returns a new UnitTest object with outputs in the testdata updated to the result of the execution of the method.")]
        [Input("test", "The unit test to rerun and assign new ouputs.")]
        [MultiOutput(0, "test", "The unit test with kept method and inputs, but replaced outputs to the result of the execution of the method.")]
        [MultiOutput(1, "errors", "Any errors encountered during the execution of the method.")]
        public static Output<UT.UnitTest, List<string>> Regenerate(UT.UnitTest test)
        {
            List<UT.TestData> newTestData = new List<UT.TestData>();
            List<string> errors = new List<string>();
            MethodBase method = test.Method;
            foreach (UT.TestData data in test.Data)
            {
                var result = Run(method, data);
                //Check if no errors occured during the execution
                if (result.Item2.Count == 0)
                    newTestData.Add(new UT.TestData(data.Inputs, result.Item1));
                else
                    errors.AddRange(result.Item2);
            }

            return new Output<UT.UnitTest, List<string>>
            {
                Item1 = new UT.UnitTest { Method = method, Data = newTestData },
                Item2 = errors
            };
        }

        /***************************************************/
    }
}
