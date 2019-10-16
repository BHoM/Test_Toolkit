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
        public static ComplianceResult IsValidNamespace(NamespaceDeclarationSyntax node, CodeContext ctx)
        {
            string name = node.Name.ToString();
            if (ctx != null && string.IsNullOrWhiteSpace(ctx.Namespace)) name = ctx.Namespace + name;
            if(name.StartsWith("BH."))
            {
                string[] parts = name.Split('.');
                string second = parts[1];

                if (!(second == "oM" || second == "Engine" || second == "Adapter" || second == "UI"))
                {
                    return Create.ComplianceResult(
                        ResultStatus.CriticalFail,
                        new List<Error> {
                            Create.Error($"Namespace '{name}' is not a valid BHoM namespace", Create.Span(node.Name.Span.Start, node.Name.Span.Length))
                        }
                    );
                }
            }
            return Create.ComplianceResult(ResultStatus.Pass);
        }

    }
}
