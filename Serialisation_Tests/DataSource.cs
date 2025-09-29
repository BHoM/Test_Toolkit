using BH.Engine.Base;
using BH.Engine.Diffing;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using BH.Engine.Test;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Test;
using BH.oM.Test.Results;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Serialisation
{
    public static class DataSource
    {

        public static IEnumerable<Type> OmTypes()
        {
            return OmTypesToTest(Setup.Query.CurrentAssemblies());
        }

        public static IEnumerable<MethodBase> EngineMethods()
        {
            return EngineMethodsToTest(Setup.Query.CurrentAssemblies());
        }

        /*************************************/

        public static List<Type> OmTypesToTest(List<Assembly> assembliesToTest)
        {
            assembliesToTest = assembliesToTest.Where(x => x.IsOmAssembly()).ToList();

            // It feels like the BHoMTypeList method should already return a clean list of Type but it doesn't at the moment
            return assembliesToTest.SelectMany(a => a.GetTypes().Where(x => {
                return typeof(IObject).IsAssignableFrom(x)
                  && !x.IsAbstract
                  && !x.IsDeprecated()
                  && !x.GetProperties().Select(p => p.PropertyType.Namespace).Any(n => !n.StartsWith("BH.") && !n.StartsWith("System"));
            })).ToList();
        }

        /*************************************/

        public static List<MethodInfo> EngineMethodsToTest(List<Assembly> assembliesToTest)
        {
            assembliesToTest = assembliesToTest.Where(x => x.IsEngineAssembly()).ToList();
            return BH.Engine.Base.Query.BHoMMethodList().Where(x => assembliesToTest.Any(a => x.DeclaringType.Assembly == a)).ToList();
        }

        /*************************************/

    }
}
