/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BH.oM.Base;

namespace BH.oM.Test.Interoperability.Settings
{
    [Description("All settings needed to run a PushPullCompare for a specific adapter.")]
    public class InteropabilityTestSettings : BHoMObject
    {
        [Description("The type of adapter to test.")]
        public virtual Type AdapterType { get; set; }

        [Description("The arguments required by the constructor of the adapter with the largest set of arguments.")]
        public virtual List<object> AdapterConstructorArguments { get; set; } = new List<object>();

        [Description("The types of objects to be tested")]
        public virtual List<Type> TestTypes { get; set; } = new List<Type>();

        [Description("Config to be used for the PushPullCompare method.")]
        public virtual PushPullCompareConfig PushPullConfig { get; set; }
    }
}





