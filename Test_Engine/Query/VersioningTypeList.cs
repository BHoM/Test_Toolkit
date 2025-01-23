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

using BH.Engine.Reflection;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Ensures all assemblies are loaded and returns a list of types to be included when generating a Versioning test set.")]
        [Input("includeRevit", "Toggle to control wheter assemblies depending on revit API dlls should be loaded or not.")]
        [MultiOutput(0, "includedTypes", "Types to be considered for adding to the Versioning TestSet.")]
        [MultiOutput(1, "ignoredTypes", "Types to _not_ be added to the Versioning TestSet.")]
        public static Output<List<Type>, List<Type>> VersioningTypeList(bool includeRevit = true)
        {
            BH.Engine.Base.Compute.LoadAllAssemblies();

            if (includeRevit)
            {
                if (!Compute.LoadRevitAssemblies(true))
                {
                    BH.Engine.Base.Compute.RecordError($"Exiting {nameof(VersioningTypeList)} with empty lists returned as failed to execute {nameof(Compute.LoadRevitAssemblies)}.");
                    return new Output<List<Type>, List<Type>>() { Item1 = new List<Type>(), Item2 = new List<Type>() };
                }
            }

            List<Type> bhomTypeList = BH.Engine.Base.Query.BHoMTypeList();

            List<Type> includeTypeList = new List<Type>();
            List<Type> ignoredTypeList = new List<Type>();

            foreach (Type type in bhomTypeList)
            {
                bool isDepracted = false;
                try
                {
                    isDepracted = type.IsDeprecated();
                }
                catch (Exception e)
                {
                    isDepracted = true;
                    BH.Engine.Base.Compute.RecordError(e, $"Could not check if {type.FullName} was deprecated");
                }
                if (!type.IsTestToolkit() && typeof(IObject).IsAssignableFrom(type) && !type.IsAbstract && !isDepracted)
                {
                    includeTypeList.Add(type);
                }
                else
                    ignoredTypeList.Add(type);
            }

            return new Output<List<Type>, List<Type>> { Item1 = includeTypeList, Item2 = ignoredTypeList };
        }

        /***************************************************/
    }
}

