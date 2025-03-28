/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.Engine.Test.NUnit
{
    public static partial class Query
    {
        [System.ComponentModel.Description("Returns all types in an assembly that contain any methods with NUnit 'Test' attribute.")]
        [Input("assembly", "Assembly to parse in search for NUnit test types.")]
        [Output("classes", "All types that contain methods with NUnit 'Test' attribute.")]
        public static List<Type> NUnitTestClasses(this Assembly assembly)
        {
            return assembly.NUnitTestMethods().Select(x => x.DeclaringType).Distinct().ToList();
        }
    }
}


