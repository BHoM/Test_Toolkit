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
using BH.oM.Test.CodeCompliance.Attributes;

namespace BH.Engine.Test.CodeCompliance
{
    public static partial class Query
    {
        public static bool IPasses(this ConditionAttribute condition, SyntaxNode node)
        {
            if (condition == null || node == null)
                return false;

            return Passes(condition as dynamic, node as dynamic);
        }

        public static bool Passes(this ConditionAttribute condition, SyntaxNode node)
        {
            if (condition == null)
                return false;

            return condition.Expect;
        }

        public static bool Passes(this PathAttribute condition, SyntaxNode node)
        {
            if (condition == null || node == null)
                return false;

            return condition.Pattern.IsMatch(node.SyntaxTree.FilePath) == condition.Expect;
        }

        public static bool Passes(this IsPublicAttribute condition, MemberDeclarationSyntax node)
        {
            if (condition == null || node == null)
                return false;

            return node.IsPublic() == condition.Expect;
        }
    }
}







