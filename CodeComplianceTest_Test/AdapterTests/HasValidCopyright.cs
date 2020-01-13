using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;
using BH.Engine.CodeComplianceTest;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.IO;

namespace BH.Test.Test
{
    public partial class Test_Adapter
    {
        [TestMethod]
        public void HasValidCopyright()
        {
            List<string> changedFiles = GetChangedObjectFiles();
            if (changedFiles == null) { Assert.IsTrue(true); return; }

            ComplianceResult r = Create.ComplianceResult(ResultStatus.Pass);
            foreach (string s in changedFiles)
            {
                StreamReader sr = new StreamReader(s);
                string file = sr.ReadToEnd();
                sr.Close();

                if (file != null)
                {
                    SyntaxTree st = BH.Engine.CodeComplianceTest.Convert.ToSyntaxTree(file, s);
                    r = r.Merge(BH.Engine.CodeComplianceTest.Query.HasValidCopyright((st.GetRoot() as CompilationUnitSyntax).GetLeadingTrivia(), DateTime.Now.Year));
                }
            }

            if (r.Status == ResultStatus.CriticalFail)
                Assert.Fail(r.Errors.Select(x => x.ToText() + "\n").Aggregate((a, b) => a + b));
            else
                Assert.IsTrue(true);
        }
    }
}
