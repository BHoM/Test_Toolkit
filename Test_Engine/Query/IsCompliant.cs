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
        public static ComplianceResult IIsCompliant(this SyntaxNode node, CodeContext ctx = null)
        {
            if (ctx == null) ctx = new CodeContext();
            else ctx = new CodeContext { Namespace = ctx.Namespace, Class = ctx.Class, Method = ctx.Method };
            return IsCompliant(node as dynamic, ctx);
        }

        public static ComplianceResult IsCompliant(this SyntaxNode node, CodeContext ctx)
        {
            return Create.ComplianceResult("", ResultStatus.Pass);
        }

        public static ComplianceResult IsCompliant<T>(this SyntaxList<T> syntaxList, CodeContext ctx) where T : SyntaxNode
        {

            ComplianceResult finalresult = Create.ComplianceResult("", ResultStatus.Pass);
            foreach(SyntaxNode syntaxNode in syntaxList)
            {
                var result = syntaxNode.IIsCompliant();
                finalresult = finalresult.Merge(result);
            }
            return finalresult;
        }

        public static ComplianceResult IsCompliant(this CompilationUnitSyntax node, CodeContext ctx)
        {
            var result = Compute.RunChecks(node, ctx);
            return result
                .Merge(node.Members.IsCompliant(ctx))
                .Merge(node.Usings.IsCompliant(ctx));
        }

        public static ComplianceResult IsCompliant(this NamespaceDeclarationSyntax node, CodeContext ctx)
        {
            ComplianceResult result = Compute.RunChecks(node, ctx);
            if (string.IsNullOrWhiteSpace(ctx.Namespace))
            {
                ctx.Namespace = node.Name.ToString();
            }
            else
            {
                ctx.Namespace += $".{node.Name.ToString()}";
            }
            return result
                .Merge(node.Members.IsCompliant(ctx))
                .Merge(node.Usings.IsCompliant(ctx));
        }

        public static ComplianceResult IsCompliant(this ClassDeclarationSyntax node, CodeContext ctx)
        {
            ComplianceResult result = Compute.RunChecks(node, ctx);
            if (string.IsNullOrWhiteSpace(ctx.Class))
            {
                ctx.Class = node.Identifier.Text;
            }
            else
            {
                ctx.Class += $".{node.Identifier.Text}";
            }
            return result.Merge(node.Members.IsCompliant(ctx));
        }
    }
}
