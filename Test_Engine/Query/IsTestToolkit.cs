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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks if a type is defined in an assembly that is part of Test_Toolkit.")]
        [Input("type", "Type to check.")]
        [Output("isTestToolkit", "Returns true if the type is defined inside Test_Toolkit.")]
        public static bool IsTestToolkit(this Type type)
        {
            if (type == null)
                return false;

            string assemblyName = type.Assembly?.GetName()?.Name;
            
            return m_testToolkitAssemblyNames.Contains(assemblyName);
        }

        /***************************************************/

        [Description("Checks if a method is defined in an assembly that is part of Test_Toolkit.")]
        [Input("method", "Method to check.")]
        [Output("isTestToolkit", "Returns true if the method is defined inside Test_Toolkit.")]
        public static bool IsTestToolkit(this MethodBase method)
        {
            if (method == null)
                return false;

            return method.DeclaringType.IsTestToolkit();
        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static HashSet<string> m_testToolkitAssemblyNames = new HashSet<string> 
        { 
            "CodeComplianceTest_Engine", 
            "CodeComplianceTest_oM", 
            "InteroperabilityTest_Engine", 
            "InteroperabilityTest_oM", 
            "NUnit_Engine", 
            "NUnit_oM", 
            "Test_Engine", 
            "TestRunner", 
            "UnitTest_Engine", 
            "UnitTest_oM" 
        };

        /***************************************************/
    }
}
