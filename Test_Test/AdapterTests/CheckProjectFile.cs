using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Test;
using BH.Engine.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.IO;

namespace BH.Test.Test
{
    public partial class Test_Adapter
    {
        [TestMethod]
        public void CheckProjectFile()
        {
            List<string> changedFiles = GetChangedObjectFiles();
            if (changedFiles == null || changedFiles.Where(x => Path.GetExtension(x).EndsWith("csproj")).Count() == 0) { Assert.IsTrue(true); return; }

            changedFiles = changedFiles.Where(x => Path.GetExtension(x).EndsWith("csproj")).ToList();

            ComplianceResult r = Create.ComplianceResult(ResultStatus.Pass);
            foreach (string s in changedFiles)
            {
                r = r.Merge(BH.Engine.Test.Compute.CheckReferences(s));
            }

            if (r.Status == ResultStatus.CriticalFail)
                Assert.Fail(r.Errors.Select(x => x.ToText() + "\n").Aggregate((a, b) => a + b));
            else
                Assert.IsTrue(true);
        }
    }
}
