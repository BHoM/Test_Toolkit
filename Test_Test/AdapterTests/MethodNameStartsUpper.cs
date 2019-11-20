using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Test
{
    public partial class Test_Adapter
    {
        [TestMethod]
        public void MethodNameStartsUpper()
        {
            Test.RunTest("MethodNameStartsUpper", GetChangedObjectFiles());
        }
    }
}
