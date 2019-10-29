/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test
{
    public static partial class Query
    {
        public static ComplianceResult IIsCompliant(this SyntaxNode node)
        {
            return IsCompliant(node as dynamic);
        }

        public static ComplianceResult IsCompliant(this SyntaxNode node)
        {
            return Compute.RunChecks(node);
        }

        public static ComplianceResult IsCompliant<T>(this SyntaxList<T> syntaxList) where T : SyntaxNode
        {
            ComplianceResult finalresult = Create.ComplianceResult(ResultStatus.Pass);
            foreach(SyntaxNode syntaxNode in syntaxList)
            {
                var result = syntaxNode.IIsCompliant();
                finalresult = finalresult.Merge(result);
            }
            return finalresult;
        }
        
        public static ComplianceResult IsCompliant<T>(this SeparatedSyntaxList<T> syntaxList) where T : SyntaxNode
        {
            ComplianceResult finalresult = Create.ComplianceResult(ResultStatus.Pass);
            foreach(SyntaxNode syntaxNode in syntaxList)
            {
                var result = syntaxNode.IIsCompliant();
                finalresult = finalresult.Merge(result);
            }
            return finalresult;
        }

        public static ComplianceResult IsCompliant(this CompilationUnitSyntax node)
        {
            var result = Compute.RunChecks(node);
            return result
                .Merge(node.Members.IsCompliant())
                .Merge(node.Usings.IsCompliant());
        }

        public static ComplianceResult IsCompliant(this NamespaceDeclarationSyntax node)
        {
            ComplianceResult result = Compute.RunChecks(node);
            return result
                .Merge(node.Members.IsCompliant())
                .Merge(node.Usings.IsCompliant());
        }

        public static ComplianceResult IsCompliant(this ClassDeclarationSyntax node)
        {
            ComplianceResult result = Compute.RunChecks(node);
            return result.Merge(node.Members.IsCompliant());
        }

        public static ComplianceResult IsCompliant(this BaseMethodDeclarationSyntax node)
        {
            return Compute.RunChecks(node)
                .Merge(node.AttributeLists.IsCompliant())
                .Merge(node.ParameterList.Parameters.IsCompliant());
        }
        
        public static ComplianceResult IsCompliant(this AttributeListSyntax node)
        {
            return node.Attributes.IsCompliant(); 
        }
    }
}
