/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Test.CodeCompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Convert
    {
        public static Span ToSpan(this LineSpan lineSpan, string context)
        {
            if (lineSpan == null)
                return Create.Span(1, 1);

            int start = lineSpan.Start.ToPosition(context);
            int end = lineSpan.End.ToPosition(context);
            return Create.Span(start, end - start);
        }

        public static Span ToSpan(this Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            if (span == null)
                return Create.Span(1, 1);

            return Create.Span(span.Start, span.Length);
        }
    }
}


