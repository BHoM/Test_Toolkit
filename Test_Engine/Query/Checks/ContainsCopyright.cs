using BH.oM.Test;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Test.Checks
{
    public static partial class Query
    {
        public static ComplianceResult ContainsCopyright(CompilationUnitSyntax node, CodeContext ctx)
        {
            return Test.Query.ContainsCopyright(node.GetLeadingTrivia(), "");
        }

    }
}
