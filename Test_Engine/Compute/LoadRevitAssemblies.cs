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
using System.IO;
using System.Linq;

namespace BH.Engine.Test
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks that Revit API dlls exists, and load up all revit assemblies.")]
        [Input("doLoad", "Set to true to check if all requirements are set and laod up revid assemblies.")]
        [Output("success", "Returns true if all requirements where fullfilled and the assemblies where atempted to be loaded.")]
        public static bool LoadRevitAssemblies(bool doLoad)
        {
            if (!doLoad)
                return false;

            string bhomAssembliesFolder = BH.Engine.Base.Query.BHoMFolder();

            if (!Directory.Exists(bhomAssembliesFolder))
            {
                BH.Engine.Base.Compute.RecordError("Cant find assemblies folder");
                return false;
            }

            var revitDLLs = new List<string>()
            {
                "AdWindows.dll",
                "RevitAPI.dll",
                "RevitAPIUI.dll",
                "UIFramework.dll",
                "UIFrameworkServices.dll",
            };

            foreach (string revitDll in revitDLLs)
            {
                if (!File.Exists(Path.Combine(bhomAssembliesFolder, revitDll)))
                {
                    BH.Engine.Base.Compute.RecordError($"{revitDll} is missing from the {bhomAssembliesFolder} folder. Please make sure you have built the RevitAPIMocks.");
                    return false;
                }
            }

            foreach (string assembly in Directory.EnumerateFiles(@"C:\ProgramData\BHoM\Assemblies", "*Revit*.dll", SearchOption.AllDirectories))
            {
                BH.Engine.Base.Compute.LoadAssembly(assembly);
            }

            return true;

        }

        /***************************************************/
    }
}
