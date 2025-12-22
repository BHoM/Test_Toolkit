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

using BH.oM.Test.CodeCompliance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        public static string IGetClass(this SyntaxNode node)
        {
            if (node == null)
                return null;

            return GetClass(node as dynamic);
        }

        public static string GetClass(this SyntaxNode node)
        {
            return node == null || node.Parent == null ? null : node.Parent.IGetClass();
        }
        public static string GetClass(this ClassDeclarationSyntax node)
        {
            if (node == null)
                return null;

            string within = node.Parent.IGetClass();

            if (string.IsNullOrEmpty(within))
                return node.IGetName();

            return within + "." + node.IGetName();
        }
    }
}







