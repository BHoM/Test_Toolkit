using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.oM.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class IsPublicAttribute : ConditionAttribute, IImmutable
    {
        public IsPublicAttribute(bool expect = true) : base(expect)
        {
        }
    }
}
