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

using BH.Engine.Reflection;
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

        [Description("Ensures all assemblies are loaded and returns a list of Methods to be included when generating a Versioning test set.")]
        [Input("includeRevit", "Toggle to control wheter assemblies depending on revit API dlls should be loaded or not.")]
        [MultiOutput(0, "included", "Methods to be considered for adding to the Versioning TestSet.")]
        [MultiOutput(1, "ignored", "Methods to _not_ be added to the Versioning TestSet.")]
        public static Output<List<MethodInfo>, List<MethodInfo>> VersioningMethodList(bool includeRevit = true)
        {
            BH.Engine.Base.Compute.LoadAllAssemblies();

            if (includeRevit)
            {
                if (!Compute.LoadRevitAssemblies(true))
                {
                    BH.Engine.Base.Compute.RecordError($"Exiting {nameof(VersioningMethodList)} with empty lists returned as failed to execute {nameof(Compute.LoadRevitAssemblies)}.");
                    return new Output<List<MethodInfo>, List<MethodInfo>>() { Item1 = new List<MethodInfo>(), Item2 = new List<MethodInfo>() };
                }
            }

            List<MethodInfo> bhomMethodList = BH.Engine.Base.Query.BHoMMethodList();

            List<MethodInfo> included = new List<MethodInfo>();
            List<MethodInfo> ignored = new List<MethodInfo>();

            foreach (MethodInfo method in bhomMethodList)
            {
                if(!method.IsTestToolkit())
                    included.Add(method);
                else
                    ignored.Add(method);
            }

            return new Output<List<MethodInfo>, List<MethodInfo>> { Item1 = included, Item2 = ignored };
        }

        /***************************************************/
    }
}
