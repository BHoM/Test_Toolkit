using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Test;
using BH.Engine.CodeComplianceTest;

using Microsoft.CodeAnalysis;

namespace BH.Test.Test
{
    public static partial class Test
    {

        public static void RunTest(string name, List<string> files)
        {
            List<string> changedFiles = files;
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
                    List<System.Reflection.MethodInfo> o = Query.AllChecks().ToList();
                    foreach (var check in o.Where(x => x.Name == name))
                    {
                        r = r.Merge(check.Check(st.GetRoot()));
                    }
                }
            }

            if (r.Status == ResultStatus.CriticalFail)
                Assert.Fail(r.Errors.Select(x => x.ToText() + "\n").Aggregate((a, b) => a + b));
            else
                Assert.IsTrue(true);
        }
    }
}
