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
using BH.oM.Test;
using BH.oM.Test.Results;
using System.Reflection;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets all inner TestInformation of a particular type. Should typically be called without providing a type, but specifying the generics when called from the code.")]
        [Input("testResult", "The results to fetch inner results from.")]
        [Input("searchDeep", "When set to true, inner TestResults are also scanned for information of the provided type.")]
        [Input("type", "The type of inner result to fetch. Can be left to null when the method is called from the code.")]
        [Output("information", "Information of the type.")]
        public static List<T> TestInformationOfType<T>(this TestResult testResult, bool searchDeep = true, Type type = null) where T : ITestInformation
        {
            //Type input here to be able to call it from the UI
            //Check if provided type is null or different from the type restriction on the generics
            if (type != null && type != typeof(T))
            {
                if (!typeof(T).IsAssignableFrom(type))
                {
                    Engine.Base.Compute.RecordError($"Type {type.ToString()} is not assignable to {typeof(T).ToString()}. Cannot extract inner results.");
                    return null;
                }
                //if T is different than the provided type

                var method = typeof(Query)
                            .GetMethods()
                            .Single(m => m.Name == "TestInformationOfType" && m.IsGenericMethodDefinition && m.GetParameters().Count() == 3);

                //Make the method into a generic and call via reflection to impose the generic constraints of the provided type
                MethodInfo generic = method.MakeGenericMethod(new Type[] { type });
                return ((IEnumerable<object>)generic.Invoke(null, new object[] { testResult, searchDeep, null })).Cast<T>().ToList();

            }

            //If T and type are the same or type is null, no reflection needed, can continue with the default running below using the generic restriction.

            List<T> information = testResult.Information.OfType<T>().ToList();

            if (searchDeep)
            {
                foreach (TestResult innerResult in testResult.Information.OfType<TestResult>())
                {
                    information.AddRange(innerResult.TestInformationOfType<T>(searchDeep));
                }
            }
            return information;
        }

        /***************************************************/

    }
}

