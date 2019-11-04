using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Profiling
{
    internal static partial class Utils
    {
        internal static List<IBHoMObject> GenerateRandomObjects(Type t, int count)
        {
            List<IBHoMObject> objs = new List<IBHoMObject>();

            for (int i = 0; i < count; i++)
            {
                objs.Add(BH.Engine.Base.Create.RandomObject(t) as dynamic);
            }

            return objs;
        }
    }
}
