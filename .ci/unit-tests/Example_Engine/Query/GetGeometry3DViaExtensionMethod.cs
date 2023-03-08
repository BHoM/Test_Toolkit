/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.Runtime.CompilerServices;
using BH.oM.Geometry;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Structure.Elements;
using BH.oM.Structure.SectionProperties;

namespace BH.Engine.Example
{
    public static partial class Query
    {
        public static IGeometry GetGeometry3DViaExtensionMethod(this ConcreteSection section)
        {
            // If structure_engine is not referenced, this should fail to find a method:
            System.Reflection.MethodInfo mi = BH.Engine.Base.Query.ExtensionMethodToCall(section, "Geometry"); 
            if (mi != null)
                return BH.Engine.Base.Compute.RunExtensionMethod(section, "Geometry") as IGeometry;
            else
                return null;
        }
    }
}