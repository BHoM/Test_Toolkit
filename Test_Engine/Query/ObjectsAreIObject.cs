﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Test;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        public static List<string> ObjectsAreIObject()
        {
            BH.Engine.Reflection.Compute.LoadAllAssemblies();

            Type iObjectType = typeof(IObject);
            List<Type> errors = new List<Type>();

            //Test each object individually
            foreach (Type type in BH.Engine.Reflection.Query.BHoMTypeList())
            {
                if (type.IsEnum || type.IsAbstract || type.FullName.EndsWith("Properties.Settings") || type.FullName.EndsWith("Properties.Resources"))
                    continue;

                if (!iObjectType.IsAssignableFrom(type))
                    errors.Add(type);
            }

            List<string> errorOut = new List<string>();
            foreach(Type t in errors)
                errorOut.Add(t.FullName + " is not an IObject");

            return errorOut;
        }
    }
}
