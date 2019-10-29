using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class ConditionAttribute : Attribute, IImmutable
    {
        public bool Expect { get; } = true;
        public ConditionAttribute(bool expect = true)
        {
            Expect = expect;
        }
    }
}
