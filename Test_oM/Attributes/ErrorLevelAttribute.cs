using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ErrorLevelAttribute : Attribute, IImmutable
    {
        public ErrorLevel Level { get; }

        public ErrorLevelAttribute(ErrorLevel level)
        {
            Level = level;
        }
    }
}
