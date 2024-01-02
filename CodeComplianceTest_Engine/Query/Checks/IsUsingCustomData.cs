/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Test.CodeCompliance.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;

namespace BH.Engine.Test.CodeCompliance.Checks
{
    public static partial class Query
    {
        [Message("The use of CustomData within the code is discouraged except in circumstances where volatile data is being used.", "IsUsingCustomData")]
        [ErrorLevel(TestStatus.Warning)]
        [Path(@"([a-zA-Z0-9]+)_(Engine|Adapter)\\.*\.cs$")]
        [Path(@"([a-zA-Z0-9]+)_Engine\\Objects\\.*\.cs$", false)]
        [Path(@"([a-zA-Z0-9]+)_Tests\\.*\.cs$", false)]
        [ComplianceType("code")]
        public static Span IsUsingCustomData(this StatementSyntax node)
        {
            if (node == null)
                return null;

            List<Type> typesToCheck = new List<Type>
            {
                typeof(BreakStatementSyntax),
                typeof(CheckedStatementSyntax),
                typeof(CommonForEachStatementSyntax),
                typeof(ContinueStatementSyntax),
                typeof(DoStatementSyntax),
                typeof(EmptyStatementSyntax),
                typeof(FixedStatementSyntax),
                typeof(ForEachStatementSyntax),
                typeof(ForStatementSyntax),
                typeof(GotoStatementSyntax),
                typeof(IfStatementSyntax),
                typeof(LabeledStatementSyntax),
                typeof(LocalDeclarationStatementSyntax),
                typeof(LockStatementSyntax),
                typeof(ReturnStatementSyntax),
                typeof(SwitchStatementSyntax),
                typeof(ThrowStatementSyntax),
                typeof(TryStatementSyntax),
                typeof(WhileStatementSyntax),
                typeof(YieldStatementSyntax),
            };

            if (!typesToCheck.Contains(node.GetType()))
                return null;

            if (node.ToString().Contains("CustomData"))
                return node.Span.ToSpan();

            return null;
        }
    }
}





