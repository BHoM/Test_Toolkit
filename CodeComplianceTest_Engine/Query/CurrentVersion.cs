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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        public static string CurrentAssemblyVersion()
        {
            return "8.0"; //Update each year - don't forget the one below!
        }

        public static string FullCurrentAssemblyVersion()
        {
            return $"{CurrentAssemblyVersion()}.0.0";
        }

        public static string CurrentAssemblyFileVersion()
        {
            return "8.2"; //Update each milestone - don't forget the one above!
        }

        public static string FullCurrentAssemblyFileVersion()
        {
            return $"{CurrentAssemblyFileVersion()}.0.0";
        }
    }
}



