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
    public class PathAttribute : ConditionAttribute, IImmutable
    {
        public Regex Pattern { get; }

        public PathAttribute(string pattern, bool expect = true) : base(expect)
        {
            Pattern = new Regex(pattern);
        }
    }
}
