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

using System.Reflection;
using BH.oM.Base.Attributes;
using BH.oM.Test.CodeCompliance;
using BH.oM.Quantities.Attributes;

namespace BH.Engine.Test.CodeCompliance.DynamicChecks
{
    public static partial class Query
    {
        public static bool IsValidQuantityInputAttribute(MemberInfo node)
        {
            if (node == null)
                return true;

            List<InputAttribute> inputAttributes = node.GetCustomAttributes<InputAttribute>().ToList();

            foreach(InputAttribute ia in inputAttributes)
            {
                if(ia.Classification != null)
                {
                    if (!typeof(ClassificationAttribute).IsAssignableFrom(ia.Classification.GetType()))
                        return false;
                }
            }

            return true;
        }
    }
}





