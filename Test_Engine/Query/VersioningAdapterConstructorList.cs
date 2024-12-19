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
using System.Reflection;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Ensures all assemblies are loaded and returns a list of AdapterConstructors to be included when generating a Versioning test set.")]
        [Input("includeRevit", "Toggle to control wheter assemblies depending on revit API dlls should be loaded or not.")]
        [MultiOutput(0, "included", "AdapterConstructors to be considered for adding to the Versioning TestSet.")]
        [MultiOutput(1, "ignored", "AdapterConstructors to _not_ be added to the Versioning TestSet.")]
        public static Output<List<ConstructorInfo>, List<ConstructorInfo>> VersioningAdapterConstructorList(bool includeRevit = true)
        {
            BH.Engine.Base.Compute.LoadAllAssemblies();

            if (includeRevit)
            {
                if (!Compute.LoadRevitAssemblies(true))
                {
                    BH.Engine.Base.Compute.RecordError($"Exiting {nameof(VersioningAdapterConstructorList)} with empty lists returned as failed to execute {nameof(Compute.LoadRevitAssemblies)}.");
                    return new Output<List<ConstructorInfo>, List<ConstructorInfo>>() { Item1 = new List<ConstructorInfo>(), Item2 = new List<ConstructorInfo>() };
                }
            }

            List<ConstructorInfo> bhomAdapterConstructors = BH.Engine.Base.Query.AdapterTypeList().SelectMany(x => x.GetConstructors()).ToList();

            List<ConstructorInfo> included = new List<ConstructorInfo>();
            List<ConstructorInfo> ignored = new List<ConstructorInfo>();

            foreach (var ctor in bhomAdapterConstructors) 
            { 
                if(ctor.IsNotImplemented() || ctor.IsDeprecated())
                    ignored.Add(ctor);
                else
                    included.Add(ctor);
            }

            return new Output<List<ConstructorInfo>, List<ConstructorInfo>> { Item1 = included, Item2 = ignored };
        }

        /***************************************************/
    }
}

