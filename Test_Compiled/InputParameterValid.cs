using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using BH.Engine.Reflection;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BH.Engine.Test;

namespace BH.Test.TestCompiled
{
    public partial class TestCompiledCompliance
    {
        [TestMethod]
        public void InputParameterValid()
        {
            BH.Engine.Reflection.Compute.LoadAllAssemblies(GetBuildFolder());

            List<MethodInfo> errors = new List<MethodInfo>();

            foreach (MethodInfo method in BH.Engine.Reflection.Query.BHoMMethodList())
            {
                if (method.IsInputParameterValid() != null && method.IsInputParameterValid().Count > 0)
                    errors.Add(method);
            }

            if (errors.Count > 0)
            {
                string message = errors.Select(x => "\n - " + x.DeclaringType.ToText(true) + "." + x.Name + '(' + x.GetParameters().Select(y => BH.Engine.Reflection.Convert.ToText(y.ParameterType) + ' ' + y.Name).Aggregate((a, b) => a + ", " + b) + ") - parameter input should start with a lowercase character").Aggregate((x, y) => x + y);
                Assert.Fail(message);
            }
            else
                Assert.IsTrue(true);
        }
    }
}
