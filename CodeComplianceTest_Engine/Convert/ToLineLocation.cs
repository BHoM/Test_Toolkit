/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Convert
    {
        public static LineLocation ToLineLocation(int position, string context)
        {
            if (position > context.Length)
            {
                Reflection.Compute.RecordError($"{position} not found in context");
                return null;
            }
            if (position > context.Length) throw new ArgumentException($"{position} not found in context");
            string beforeStart = context.Substring(0, position);
            int line = beforeStart.Count((c) => c == '\n') + 1;
            int column = position - (beforeStart.LastIndexOf('\n') + 1);
            return Create.LineLocation(line, column);
        }
    }
}

